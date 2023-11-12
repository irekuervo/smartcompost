# smartcompost/app_pkg/views/measurements.py

from flask import Blueprint, jsonify
from ..models import Measurement

measurements_bp = Blueprint("measurements", __name__, url_prefix="/api/measurements")


@measurements_bp.route("/", methods=["GET"])
def get_measurements():
    try:
        # Aquí puedes implementar la lógica para obtener las mediciones, por ejemplo:
        measurements = Measurement.query.all()
        measurements_data = [{"id": m.id, "value": m.value, "timestamp": m.timestamp} for m in measurements]
        return jsonify({"measurements": measurements_data})
    except Exception as e:
        return jsonify({"error": str(e)}), 500


@measurements_bp.route("/latest", methods=["GET"])
def get_latest_measurement():
    try:
        # Aquí puedes implementar la lógica para obtener la última medición, por ejemplo:
        latest_measurement = Measurement.query.order_by(Measurement.timestamp.desc()).first()
        if latest_measurement:
            latest_measurement_data = {"id": latest_measurement.id, "value": latest_measurement.value,
                                       "timestamp": latest_measurement.timestamp}
            return jsonify(latest_measurement_data)
        else:
            return jsonify({"message": "No se encontraron mediciones"}), 404
    except Exception as e:
        return jsonify({"error": str(e)}), 500

# Puedes agregar más rutas y funciones según tus necesidades
