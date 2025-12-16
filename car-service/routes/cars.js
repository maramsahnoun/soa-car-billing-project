const express = require("express");
const router = express.Router();
const db = require("../db");

/* -----------------------------------------------------
   GET /cars
   Filtres possibles :
   /cars?categorie=SUV
   /cars?minPrice=50&maxPrice=200
   ----------------------------------------------------- */
router.get("/", async (req, res) => {
    const { categorie, minPrice, maxPrice } = req.query;

    let sql = "SELECT * FROM voiture WHERE 1=1";
    const params = [];

    if (categorie) {
        sql += " AND categorie = ?";
        params.push(categorie);
    }

    if (minPrice) {
        sql += " AND tarif_journalier >= ?";
        params.push(Number(minPrice));
    }

    if (maxPrice) {
        sql += " AND tarif_journalier <= ?";
        params.push(Number(maxPrice));
    }

    try {
        const [rows] = await db.query(sql, params);
        res.json(rows);
    } catch (error) {
        console.error(error);
        res.status(500).json({ error: "Erreur serveur" });
    }
});

/* -----------------------------------------------------
   GET /cars/available?start=YYYY-MM-DD&end=YYYY-MM-DD
   ----------------------------------------------------- */
router.get("/available", async (req, res) => {
    const { start, end } = req.query;

    if (!start || !end) {
        return res.status(400).json({ error: "Merci de fournir start et end" });
    }

    try {
        const [rows] = await db.query(
            `
            SELECT *
            FROM voiture v
            WHERE v.etat = 'DISPONIBLE'
            AND v.id NOT IN (
                SELECT r.voiture_id
                FROM reservation r
                WHERE r.statut <> 'ANNULEE'
                AND r.date_debut <= ?
                AND r.date_fin >= ?
            )
            `,
            [end, start]
        );

        res.json(rows);
    } catch (error) {
        console.error(error);
        res.status(500).json({ error: "Erreur lors de la recherche de disponibilité" });
    }
});

/* -----------------------------------------------------
   GET /cars/:id
   ----------------------------------------------------- */
router.get("/:id", async (req, res) => {
    try {
        const [rows] = await db.query("SELECT * FROM voiture WHERE id = ?", [
            req.params.id,
        ]);

        if (rows.length === 0) {
            return res.status(404).json({ error: "Voiture non trouvée" });
        }

        res.json(rows[0]);
    } catch (error) {
        console.error(error);
        res.status(500).json({ error: "Erreur serveur" });
    }
});

/* -----------------------------------------------------
   POST /cars  (Créer une voiture)
   ----------------------------------------------------- */
router.post("/", async (req, res) => {
    const {
        immatriculation,
        marque,
        modele,
        categorie,
        tarif_journalier,
        etat,
        description,
    } = req.body;

    try {
        await db.query(
            `
            INSERT INTO voiture
            (immatriculation, marque, modele, categorie, tarif_journalier, etat, description)
            VALUES (?, ?, ?, ?, ?, ?, ?)
            `,
            [
                immatriculation,
                marque,
                modele,
                categorie,
                tarif_journalier,
                etat || "DISPONIBLE",
                description,
            ]
        );

        res.status(201).json({ message: "Voiture ajoutée avec succès" });
    } catch (error) {
        console.error(error);
        res.status(500).json({ error: "Erreur lors de l'ajout" });
    }
});

/* -----------------------------------------------------
   PUT /cars/:id  (Modifier une voiture)
   ----------------------------------------------------- */
router.put("/:id", async (req, res) => {
    const {
        marque,
        modele,
        categorie,
        tarif_journalier,
        etat,
        description,
    } = req.body;

    try {
        await db.query(
            `
            UPDATE voiture
            SET marque = ?, modele = ?, categorie = ?, tarif_journalier = ?, etat = ?, description = ?
            WHERE id = ?
            `,
            [
                marque,
                modele,
                categorie,
                tarif_journalier,
                etat,
                description,
                req.params.id,
            ]
        );

        res.json({ message: "Voiture mise à jour" });
    } catch (error) {
        console.error(error);
        res.status(500).json({ error: "Erreur lors de la modification" });
    }
});

/* -----------------------------------------------------
   DELETE /cars/:id
   ----------------------------------------------------- */
router.delete("/:id", async (req, res) => {
    try {
        const [result] = await db.query(
            "DELETE FROM voiture WHERE id = ?",
            [req.params.id]
        );

        if (result.affectedRows === 0) {
            return res.status(404).json({ error: "Voiture non trouvée" });
        }

        res.json({ message: "Voiture supprimée" });
    } catch (error) {
        console.error(error);
        res.status(500).json({ error: "Erreur lors de la suppression" });
    }
});

module.exports = router;
