from fastapi import FastAPI, HTTPException
from pydantic import BaseModel
from datetime import date
from db import get_connection

app = FastAPI(title="Billing Service")


# ----------- Modèles Pydantic -----------

class PaymentCreate(BaseModel):
    reservation_id: int
    date_debut: date
    date_fin: date
    tarif_journalier: float
    mode_paiement: str
    supplement_options: float | None = 0.0   # ex: GPS, siège bébé


class PaymentStatusUpdate(BaseModel):
    statut: str   # "EN_ATTENTE", "PAYE", "REFUSE"


# ----------- Health & DB Check -----------

@app.get("/health")
def health():
    return {"status": "Billing service OK"}


@app.get("/db-check")
def db_check():
    try:
        conn = get_connection()
        cursor = conn.cursor()
        cursor.execute("SELECT 1")
        cursor.fetchone()
        cursor.close()
        conn.close()
        return {"db": "ok"}
    except Exception as e:
        return {"db": "error", "detail": str(e)}


# ----------- POST /payments -----------

@app.post("/payments")
def create_payment(data: PaymentCreate):
    # 1) calcul du nombre de jours
    nb_jours = (data.date_fin - data.date_debut).days
    if nb_jours <= 0:
        nb_jours = 1   # au moins 1 jour

    # 2) calcul du montant total
    montant = nb_jours * data.tarif_journalier + (data.supplement_options or 0.0)

    conn = get_connection()
    cursor = conn.cursor()

    try:
        cursor.execute(
            """
            INSERT INTO paiement (reservation_id, montant, mode_paiement, statut)
            VALUES (%s, %s, %s, %s)
            """,
            (data.reservation_id, montant, data.mode_paiement, "EN_ATTENTE")
        )
        conn.commit()
        paiement_id = cursor.lastrowid

        return {
            "message": "Paiement créé",
            "paiement_id": paiement_id,
            "reservation_id": data.reservation_id,
            "nb_jours": nb_jours,
            "montant": montant
        }
    except Exception as e:
        print("ERREUR CREATE_PAYMENT:", e)
        raise HTTPException(status_code=500, detail="Erreur lors de la création du paiement")
    finally:
        cursor.close()
        conn.close()


# ----------- GET /payments/{id} -----------

@app.get("/payments/{payment_id}")
def get_payment(payment_id: int):
    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    try:
        cursor.execute("SELECT * FROM paiement WHERE id = %s", (payment_id,))
        paiement = cursor.fetchone()

        if not paiement:
            raise HTTPException(status_code=404, detail="Paiement non trouvé")

        return paiement
    except Exception as e:
        print("ERREUR GET_PAYMENT:", e)
        raise HTTPException(status_code=500, detail="Erreur lors de la récupération du paiement")
    finally:
        cursor.close()
        conn.close()


# ----------- GET /payments/by-reservation/{reservation_id} -----------

@app.get("/payments/by-reservation/{reservation_id}")
def get_payment_by_reservation(reservation_id: int):
    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    try:
        cursor.execute(
            "SELECT * FROM paiement WHERE reservation_id = %s",
            (reservation_id,)
        )
        paiement = cursor.fetchone()

        if not paiement:
            raise HTTPException(
                status_code=404,
                detail="Aucun paiement pour cette réservation"
            )

        return paiement
    except Exception as e:
        print("ERREUR GET_PAYMENT_BY_RES:", e)
        raise HTTPException(status_code=500, detail="Erreur lors de la récupération du paiement")
    finally:
        cursor.close()
        conn.close()


# ----------- PATCH /payments/{id}/status -----------

@app.patch("/payments/{payment_id}/status")

@app.patch("/payments/{payment_id}/status")
def update_payment_status(payment_id: int, data: PaymentStatusUpdate):
    if data.statut not in ["EN_ATTENTE", "SUCCES", "ECHEC"]:
        raise HTTPException(status_code=400, detail="Statut invalide")

    conn = get_connection()
    cursor = conn.cursor()

    try:
        cursor.execute(
            "UPDATE paiement SET statut = %s WHERE id = %s",
            (data.statut, payment_id)
        )
        conn.commit()

        if cursor.rowcount == 0:
            raise HTTPException(status_code=404, detail="Paiement non trouvé")

        return {
            "message": "Statut mis à jour",
            "paiement_id": payment_id,
            "statut": data.statut
        }
    except Exception as e:
        print("ERREUR UPDATE_STATUS:", e)
        raise HTTPException(status_code=500, detail="Erreur lors de la mise à jour du statut")
    finally:
        cursor.close()
        conn.close()


# ----------- GET /payments/{id}/invoice (facture JSON) -----------

@app.get("/payments/{payment_id}/invoice")
def get_invoice(payment_id: int):
    conn = get_connection()
    cursor = conn.cursor(dictionary=True)

    try:
        cursor.execute("SELECT * FROM paiement WHERE id = %s", (payment_id,))
        paiement = cursor.fetchone()

        if not paiement:
            raise HTTPException(status_code=404, detail="Paiement non trouvé")

        # Exemple simple de "facture" JSON
        invoice = {
            "facture_id": paiement["id"],
            "reservation_id": paiement["reservation_id"],
            "montant": float(paiement["montant"]),
            "mode_paiement": paiement["mode_paiement"],
            "statut": paiement["statut"],
            "date_paiement": str(paiement["date_paiement"]),
            "societe": "Agence de location SOA",
            "devise": "TND"
        }

        return invoice
    except Exception as e:
        print("ERREUR INVOICE:", e)
        raise HTTPException(status_code=500, detail="Erreur lors de la génération de la facture")
    finally:
        cursor.close()
        conn.close()
