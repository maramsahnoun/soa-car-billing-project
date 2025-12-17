-- Création de la base
CREATE DATABASE IF NOT EXISTS reservation_db;

-- Utilisation de la base
USE reservation_db;

-- Création de la table reservation
CREATE TABLE IF NOT EXISTS reservation (
    id              BIGINT AUTO_INCREMENT PRIMARY KEY,
    utilisateur_id  BIGINT          NOT NULL,
    voiture_id      BIGINT          NOT NULL,
    date_debut      DATE            NOT NULL,
    date_fin        DATE            NOT NULL,
    statut          ENUM('EN_ATTENTE', 'CONFIRMEE', 'ANNULEE') NOT NULL DEFAULT 'EN_ATTENTE',
    montant_total   DECIMAL(10,2)   NULL,
    date_creation   DATETIME        NOT NULL DEFAULT CURRENT_TIMESTAMP,

    INDEX idx_reservation_utilisateur (utilisateur_id),
    INDEX idx_reservation_voiture (voiture_id)
) ENGINE=InnoDB;

