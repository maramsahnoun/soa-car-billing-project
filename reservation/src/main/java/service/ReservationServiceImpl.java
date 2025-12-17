package service;
import java.util.ArrayList;
import java.util.Date;
import java.util.List;
import javax.jws.WebService;
import model.Reservation;

@WebService(endpointInterface = "service.ReservationService")
public class ReservationServiceImpl implements ReservationService {

    private static List<Reservation> reservations = new ArrayList<>();
    private static int compteur = 1;

    @Override
    public Reservation creerReservation(int clientId, int vehicleId, Date dateDebut, Date dateFin) {
        Reservation r = new Reservation(compteur++, clientId, vehicleId, dateDebut, dateFin, "ACTIVE");
        reservations.add(r);
        return r;
    }

    @Override
    public boolean annulerReservation(int reservationId) {
        for (Reservation r : reservations) {
            if (r.getId() == reservationId) {
                r.setStatut("ANNULEE");
                return true;
            }
        }
        return false;
    }

    @Override
    public List<Reservation> reservationsParClient(int clientId) {
        List<Reservation> result = new ArrayList<>();
        for (Reservation r : reservations) {
            if (r.getClientId() == clientId) result.add(r);
        }
        return result;
    }

    @Override
    public List<Reservation> reservationsParVehicule(int vehicleId) {
        List<Reservation> result = new ArrayList<>();
        for (Reservation r : reservations) {
            if (r.getVehicleId() == vehicleId) result.add(r);
        }
        return result;
    }
}
