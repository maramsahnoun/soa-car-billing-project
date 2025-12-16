CREATE TABLE IF NOT EXISTS voiture (
    id BIGINT AUTO_INCREMENT PRIMARY KEY,
    immatriculation VARCHAR(50) NOT NULL UNIQUE,
    marque VARCHAR(100) NOT NULL,
    modele VARCHAR(100) NOT NULL,
    categorie VARCHAR(50),
    tarif_journalier DECIMAL(10,2) NOT NULL,
    etat ENUM('DISPONIBLE','RESERVEE','MAINTENANCE','HORS_SERVICE')
         NOT NULL DEFAULT 'DISPONIBLE',
    description TEXT
);

-- Données de test (optionnel)
INSERT INTO voiture (immatriculation, marque, modele, categorie, tarif_journalier, etat)
VALUES
('111-TN-001','Renault','Clio','Citadine',80,'DISPONIBLE'),
('222-TN-002','Toyota','Yaris','Citadine',90,'DISPONIBLE');
