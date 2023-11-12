package main

import (
	"bytes"
	"encoding/json"
	"fmt"
	"math/rand"
	"net/http"
	"time"
)

type Measurement struct {
	ID          int     `json:"id"`
	Temperatura float64 `json:"temperatura"`
	Humedad     float64 `json:"humedad"`
	Datetime    string  `json:"datetime"`
}

type MeasurementBuilder struct {
	IDRange          [2]int
	HumidityRange    [2]float64
	TemperatureRange [2]float64
}

func NewMeasurementBuilder() *MeasurementBuilder {
	return &MeasurementBuilder{
		IDRange:          [2]int{1, 2},
		HumidityRange:    [2]float64{0.0, 100.0},
		TemperatureRange: [2]float64{-20.0, 50.0},
	}
}

func (mb *MeasurementBuilder) Build() *Measurement {
	id := rand.Intn(mb.IDRange[1]-mb.IDRange[0]+1) + mb.IDRange[0]
	humidity := rand.Float64()*(mb.HumidityRange[1]-mb.HumidityRange[0]) + mb.HumidityRange[0]
	temperature := rand.Float64()*(mb.TemperatureRange[1]-mb.TemperatureRange[0]) + mb.TemperatureRange[0]
	currentTime := time.Now().Format(time.RFC3339)

	return &Measurement{
		ID:          id,
		Humedad:     humidity,
		Temperatura: temperature,
		Datetime:    currentTime,
	}
}

func main() {
	url := "http://localhost:8080/api/compost_bins/add_measurement"
	interval := 2 * time.Second
	builder := NewMeasurementBuilder()

	// Configurar un temporizador
	ticker := time.NewTicker(interval)
	defer ticker.Stop()

	for {
		select {
		case <-ticker.C:
			// Generar una medición aleatoria usando el MeasurementBuilder
			measurement := builder.Build()

			// Convierte la medición en formato JSON
			jsonData, err := json.Marshal(measurement)
			if err != nil {
				fmt.Println("Error al serializar el struct a JSON:", err)
				continue
			}

			// Realiza la solicitud POST con la medición aleatoria
			resp, err := http.Post(url, "application/json", bytes.NewBuffer(jsonData))
			if err != nil {
				fmt.Println("Error al realizar la solicitud POST:", err)
				continue
			}
			defer resp.Body.Close()

			// Verifica el código de estado de la respuesta
			if resp.StatusCode == http.StatusCreated {
				fmt.Println("Solicitud POST exitosa")
			} else {
				fmt.Println("La solicitud POST no fue exitosa. Código de estado:", resp.StatusCode)
			}
		}
	}
}
