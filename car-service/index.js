require("dotenv").config();
const express = require("express");
const cors = require("cors");
const morgan = require("morgan");

const app = express();
const carsRoutes = require("./routes/cars");

app.use(express.json());
app.use(cors());
app.use(morgan("dev"));

// Routes
app.use("/cars", carsRoutes);

// Health check
app.get("/health", (req, res) => {
    res.send("Car service OK");
});

app.listen(3001, () => {
    console.log("Car-Service running on port 3001");
});
