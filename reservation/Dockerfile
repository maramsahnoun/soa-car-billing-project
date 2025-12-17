FROM eclipse-temurin:8-jdk

WORKDIR /app

COPY target/reservation-service-0.0.1-SNAPSHOT.jar reservation-service.jar

EXPOSE 8084

ENTRYPOINT ["java","-jar","reservation-service.jar"]
