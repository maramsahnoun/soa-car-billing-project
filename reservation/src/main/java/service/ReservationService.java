package service;

import java.util.Date;
import java.util.List;
import javax.jws.WebMethod;
import javax.jws.WebService;
import model.Reservation;

@WebService
public interface ReservationService {

    @WebMethod
    Reservation creerReservation(int clientId, int vehicleId, Date dateDebut, Date dateFin);

    @WebMethod
    boolean annulerReservation(int reservationId);

    @WebMethod
    List<Reservation> reservationsParClient(int clientId);

    @WebMethod
    List<Reservation> reservationsParVehicule(int vehicleId);
}
