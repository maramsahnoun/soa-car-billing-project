# Build stage
FROM maven:3.9-eclipse-temurin-8 AS build
WORKDIR /app
COPY pom.xml .
COPY src ./src
RUN mvn clean package -DskipTests

# Runtime stage
FROM eclipse-temurin:8-jre
WORKDIR /app
COPY --from=build /app/target/reservation-service-0.0.1-SNAPSHOT.jar reservation-service.jar
EXPOSE 8083
ENTRYPOINT ["java","-jar","reservation-service.jar"]
