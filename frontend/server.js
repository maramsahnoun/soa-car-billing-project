const express = require('express');
const axios = require('axios');
const path = require('path');

const app = express();
app.use(express.json());
app.use(express.static(path.join(__dirname, 'public')));

// Service URLs (inside Docker network)
const CAR_SERVICE = process.env.CAR_SERVICE_URL || 'http://car-service:3001';
const BILLING_SERVICE = process.env.BILLING_SERVICE_URL || 'http://billing-service:8001';
const MAINTENANCE_SERVICE = process.env.MAINTENANCE_SERVICE_URL || 'http://maintenance-service:5000';
const RESERVATION_SERVICE = process.env.RESERVATION_SERVICE_URL || 'http://reservation-service:8083';

// ============ PROXY ENDPOINTS ============

// Health check for all services
app.get('/api/health', async (req, res) => {
    const results = {
        frontend: { status: 'OK', timestamp: new Date().toISOString() },
        car: { status: 'CHECKING' },
        billing: { status: 'CHECKING' },
        maintenance: { status: 'CHECKING' },
        reservation: { status: 'CHECKING' }
    };

    // Check Car Service
    try {
        const response = await axios.get(`${CAR_SERVICE}/health`, { timeout: 3000 });
        results.car = { status: 'OK', message: response.data };
    } catch (e) {
        results.car = { status: 'ERROR', message: e.message };
    }

    // Check Billing Service
    try {
        const response = await axios.get(`${BILLING_SERVICE}/health`, { timeout: 3000 });
        results.billing = { status: 'OK', message: response.data };
    } catch (e) {
        results.billing = { status: 'ERROR', message: e.message };
    }

    // Check Maintenance Service (SOAP - just try to connect)
    try {
        await axios.get(`${MAINTENANCE_SERVICE}/MaintenanceService.asmx`, { timeout: 3000 });
        results.maintenance = { status: 'OK', message: 'SOAP Service Available' };
    } catch (e) {
        if (e.response) {
            results.maintenance = { status: 'OK', message: 'SOAP Service Available' };
        } else {
            results.maintenance = { status: 'ERROR', message: e.message };
        }
    }

    // Check Reservation Service (SOAP)
    try {
        await axios.get(`${RESERVATION_SERVICE}/reservation?wsdl`, { timeout: 3000 });
        results.reservation = { status: 'OK', message: 'SOAP Service Available' };
    } catch (e) {
        if (e.response) {
            results.reservation = { status: 'OK', message: 'SOAP Service Available' };
        } else {
            results.reservation = { status: 'ERROR', message: e.message };
        }
    }

    res.json(results);
});

// ============ CAR SERVICE PROXY ============
app.get('/api/cars', async (req, res) => {
    try {
        const response = await axios.get(`${CAR_SERVICE}/cars`);
        res.json(response.data);
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

app.get('/api/cars/:id', async (req, res) => {
    try {
        const response = await axios.get(`${CAR_SERVICE}/cars/${req.params.id}`);
        res.json(response.data);
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

app.post('/api/cars', async (req, res) => {
    try {
        const response = await axios.post(`${CAR_SERVICE}/cars`, req.body);
        res.json(response.data);
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

// ============ BILLING SERVICE PROXY ============
app.get('/api/billing/db-check', async (req, res) => {
    try {
        const response = await axios.get(`${BILLING_SERVICE}/db-check`);
        res.json(response.data);
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

app.post('/api/payments', async (req, res) => {
    try {
        const response = await axios.post(`${BILLING_SERVICE}/payments`, req.body);
        res.json(response.data);
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

app.get('/api/payments/:id', async (req, res) => {
    try {
        const response = await axios.get(`${BILLING_SERVICE}/payments/${req.params.id}`);
        res.json(response.data);
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

app.get('/api/payments/:id/invoice', async (req, res) => {
    try {
        const response = await axios.get(`${BILLING_SERVICE}/payments/${req.params.id}/invoice`);
        res.json(response.data);
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

// ============ MAINTENANCE SERVICE PROXY (SOAP) ============
app.get('/api/maintenance/all', async (req, res) => {
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <GetAllMaintenances xmlns="http://tempuri.org/" />
  </soap:Body>
</soap:Envelope>`;

    try {
        const response = await axios.post(
            `${MAINTENANCE_SERVICE}/MaintenanceService.asmx`,
            soapRequest,
            {
                headers: {
                    'Content-Type': 'text/xml; charset=utf-8',
                    'SOAPAction': '"http://tempuri.org/GetAllMaintenances"'
                }
            }
        );
        res.json({ success: true, data: response.data });
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

app.post('/api/maintenance/create', async (req, res) => {
    const { carId, type, description } = req.body;
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/">
  <soap:Body>
    <CreateMaintenance xmlns="http://tempuri.org/">
      <request>
        <CarId>${carId}</CarId>
        <Type>${type}</Type>
        <Description>${description}</Description>
      </request>
    </CreateMaintenance>
  </soap:Body>
</soap:Envelope>`;

    try {
        const response = await axios.post(
            `${MAINTENANCE_SERVICE}/MaintenanceService.asmx`,
            soapRequest,
            {
                headers: {
                    'Content-Type': 'text/xml; charset=utf-8',
                    'SOAPAction': '"http://tempuri.org/CreateMaintenance"'
                }
            }
        );
        res.json({ success: true, data: response.data });
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

// ============ RESERVATION SERVICE PROXY (SOAP) ============
app.get('/api/reservations/all', async (req, res) => {
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://service/">
  <soapenv:Body>
    <ser:getAllReservations/>
  </soapenv:Body>
</soapenv:Envelope>`;

    try {
        const response = await axios.post(
            `${RESERVATION_SERVICE}/reservation`,
            soapRequest,
            { headers: { 'Content-Type': 'text/xml; charset=utf-8' } }
        );
        res.json({ success: true, data: response.data });
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

app.post('/api/reservations/create', async (req, res) => {
    const { clientId, carId, startDate, endDate } = req.body;
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://service/">
  <soapenv:Body>
    <ser:createReservation>
      <clientId>${clientId}</clientId>
      <carId>${carId}</carId>
      <startDate>${startDate}</startDate>
      <endDate>${endDate}</endDate>
    </ser:createReservation>
  </soapenv:Body>
</soapenv:Envelope>`;

    try {
        const response = await axios.post(
            `${RESERVATION_SERVICE}/reservation`,
            soapRequest,
            { headers: { 'Content-Type': 'text/xml; charset=utf-8' } }
        );
        res.json({ success: true, data: response.data });
    } catch (e) {
        res.status(500).json({ error: e.message });
    }
});

const PORT = process.env.PORT || 8080;
app.listen(PORT, () => {
    console.log(`Frontend server running on port ${PORT}`);
});
