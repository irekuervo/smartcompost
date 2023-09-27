import React, { useState, useEffect } from "react";
import { Line } from "react-chartjs-2";

const defaultData = {
  labels: [],
  datasets: [
    {
      borderColor: "#6bd098",
      backgroundColor: "#6bd098",
      pointRadius: 0,
      pointHoverRadius: 0,
      borderWidth: 3,
      tension: 0.4,
      fill: true,
      data: [],
    },
  ],
};

const LineChart = () => {
  const [chartData, setChartData] = useState(defaultData);

  // Función para actualizar solo las partes necesarias del estado
  const updateChartData = (newLabels, newData) => {
    setChartData((prevChartData) => ({
      ...prevChartData,
      labels: newLabels,
      datasets: [
        {
          ...prevChartData.datasets[0],
          data: newData,
        },
      ],
    }));
  };

  // Función para realizar la solicitud a la API y actualizar los datos
  const fetchDataFromApi = async () => {
  
    try {
        const response = await fetch("https://localhost:7103/api/datos2");
        if (!response.ok) {
        throw new Error("Error al obtener datos de la API");
        }
        
        const data = await response.json();
        
        updateChartData(data.labels, data.datasets);
    } catch (error) {
        console.error("Error:", error);
    }
  };

    useEffect(() => {
    // Simula la actualización de datos desde la API cada 1 segundos
    const intervalId = setInterval(fetchDataFromApi, 1000);

    return () => clearInterval(intervalId);
  }, []);

  return <Line data={chartData} width={400} height={100} />;
};

export default LineChart;