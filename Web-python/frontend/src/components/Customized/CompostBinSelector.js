import React, { useState, useEffect } from "react";

const CompostBinSelector = ({ onCompostBinChange }) => {
  const [compostBinIds, setCompostBinIds] = useState([]);
  const [selectedCompostBinId, setSelectedCompostBinId] = useState("");

  useEffect(() => {
    const fetchCompostBinIds = async () => {
      try {
        const response = await fetch("http://0.0.0.0:8080/api/compost_bins/all_ids");
        if (!response.ok) {
          throw new Error("Error al obtener los IDs de las composteras");
        }

        const data = await response.json();

        setCompostBinIds(data);

        if (data.length > 0) {
          setSelectedCompostBinId(data[0]);
        }
      } catch (error) {
        console.error("Error:", error);
      }
    };

    fetchCompostBinIds();
  }, []);

  const handleCompostBinChange = (event) => {
    const selectedId = event.target.value;
    setSelectedCompostBinId(selectedId);
    onCompostBinChange(selectedId);
  };

  return (
    <div>
      <label htmlFor="compostBinSelector">Selecciona una Compostera: </label>
      <select
        id="compostBinSelector"
        value={selectedCompostBinId}
        onChange={handleCompostBinChange}
      >
        {compostBinIds.map((id) => (
          <option key={id} value={id}>
            Compost Bin {id}
          </option>
        ))}
      </select>
    </div>
  );
};

export default CompostBinSelector;
