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
        await axios.get(`${MAINTENANCE_SERVICE}/ws/maintenance`, { timeout: 3000 });
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
const MAINTENANCE_NS = 'http://maintenance-service.soa.com/2024/12';

app.get('/api/maintenance/all', async (req, res) => {
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:m="${MAINTENANCE_NS}">
  <soap:Body>
    <m:GetUpcomingMaintenances />
  </soap:Body>
</soap:Envelope>`;

    try {
        const response = await axios.post(
            `${MAINTENANCE_SERVICE}/ws/maintenance`,
            soapRequest,
            {
                headers: {
                    'Content-Type': 'text/xml; charset=utf-8',
                    'SOAPAction': `"${MAINTENANCE_NS}/IMaintenanceService/GetUpcomingMaintenances"`
                },
                timeout: 10000
            }
        );
        res.json({ success: true, data: response.data });
    } catch (e) {
        console.error('Maintenance error:', e.response?.data || e.message);
        res.status(500).json({ error: e.message, details: e.response?.data });
    }
});

app.post('/api/maintenance/create', async (req, res) => {
    const { carId, type, description } = req.body;
    const today = new Date().toISOString();
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soap:Envelope xmlns:soap="http://schemas.xmlsoap.org/soap/envelope/" xmlns:m="${MAINTENANCE_NS}">
  <soap:Body>
    <m:CreateMaintenance>
      <m:vehicleId>${carId}</m:vehicleId>
      <m:maintenanceType>${type}</m:maintenanceType>
      <m:description>${description}</m:description>
      <m:scheduledDate>${today}</m:scheduledDate>
    </m:CreateMaintenance>
  </soap:Body>
</soap:Envelope>`;

    try {
        const response = await axios.post(
            `${MAINTENANCE_SERVICE}/ws/maintenance`,
            soapRequest,
            {
                headers: {
                    'Content-Type': 'text/xml; charset=utf-8',
                    'SOAPAction': `"${MAINTENANCE_NS}/IMaintenanceService/CreateMaintenance"`
                },
                timeout: 10000
            }
        );
        res.json({ success: true, data: response.data });
    } catch (e) {
        console.error('Maintenance create error:', e.response?.data || e.message);
        res.status(500).json({ error: e.message, details: e.response?.data });
    }
});

// ============ RESERVATION SERVICE PROXY (SOAP) ============
// Note: The Java service uses French method names
app.get('/api/reservations/all', async (req, res) => {
    // Get reservations by client (use clientId=1 for demo)
    const clientId = req.query.clientId || 1;
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://service/">
  <soapenv:Body>
    <ser:reservationsParClient>
      <arg0>${clientId}</arg0>
    </ser:reservationsParClient>
  </soapenv:Body>
</soapenv:Envelope>`;

    try {
        const response = await axios.post(
            `${RESERVATION_SERVICE}/reservation`,
            soapRequest,
            { 
                headers: { 'Content-Type': 'text/xml; charset=utf-8' },
                timeout: 10000
            }
        );
        res.json({ success: true, clientId, data: response.data });
    } catch (e) {
        console.error('Reservation error:', e.response?.data || e.message);
        res.status(500).json({ error: e.message, details: e.response?.data });
    }
});

app.post('/api/reservations/create', async (req, res) => {
    const { clientId, carId, startDate, endDate } = req.body;
    // Format dates for Java Date type (use milliseconds timestamp)
    const soapRequest = `<?xml version="1.0" encoding="utf-8"?>
<soapenv:Envelope xmlns:soapenv="http://schemas.xmlsoap.org/soap/envelope/" xmlns:ser="http://service/">
  <soapenv:Body>
    <ser:creerReservation>
      <arg0>${clientId}</arg0>
      <arg1>${carId}</arg1>
      <arg2>${startDate}</arg2>
      <arg3>${endDate}</arg3>
    </ser:creerReservation>
  </soapenv:Body>
</soapenv:Envelope>`;

    try {
        const response = await axios.post(
            `${RESERVATION_SERVICE}/reservation`,
            soapRequest,
            { 
                headers: { 'Content-Type': 'text/xml; charset=utf-8' },
                timeout: 10000
            }
        );
        res.json({ success: true, data: response.data });
    } catch (e) {
        console.error('Reservation create error:', e.response?.data || e.message);
        res.status(500).json({ error: e.message, details: e.response?.data });
    }
});

const PORT = process.env.PORT || 8080;
app.listen(PORT, () => {
    console.log(`Frontend server running on port ${PORT}`);
});
