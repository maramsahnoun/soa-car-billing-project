package server;

import javax.xml.ws.Endpoint;
import service.ReservationServiceImpl;

public class ReservationServer {

    public static void main(String[] args) {
    	Endpoint.publish("http://0.0.0.0:8084/reservation", new ReservationServiceImpl());

        System.out.println("SOAP Reservation Service running...");
    }
}
