from flask import Blueprint, request, jsonify
# from sqlalchemy import func

# from models import CompostBin, Measurement

from ..application import app, db
from ..models import CompostBin, Measurement
from ..serializers import MeasurementSchema

compost_bins_bp = Blueprint('compost_bins', __name__)


# Ruta para verificar el estado de la API (health check)
@compost_bins_bp.route('/health')
def health_check():
    return jsonify({'status': 'API is healthy'})


@compost_bins_bp.route('/<int:compost_bin_id>/last_measurement')
def get_last_measurement(compost_bin_id):
    app.logger.info(f'Getting last measurement from compost bin {compost_bin_id}')

    compost_bin = CompostBin.query.get_or_404(compost_bin_id)
    last_measurement = Measurement.query.filter_by(compost_bin_id=compost_bin.compost_bin_id).order_by(
        Measurement.timestamp.desc()).first()
    if last_measurement is None:
        return jsonify({'message': 'No measurements found for this compost bin'}), 404

    # Crear una instancia del esquema MeasurementSchema y serializar el resultado
    measurement_schema = MeasurementSchema()
    measurement_data = measurement_schema.dump(last_measurement)

    return jsonify({'last_measurement': measurement_data}), 200


@compost_bins_bp.route('/<int:compost_bin_id>/measurements')
def get_measurements_by_period(compost_bin_id):
    # Parsear los parámetros del período (year, month, etc.) desde la solicitud
    # year = request.args.get('year')
    # month = request.args.get('month')
    compost_bin = CompostBin.query.get_or_404(compost_bin_id)

    # Obtén todas las mediciones asociadas al compost bin
    measurements = compost_bin.measurements

    # Serializa las mediciones utilizando el esquema
    measurement_schema = MeasurementSchema(many=True)
    measurements_data = measurement_schema.dump(measurements)

    return jsonify(measurements_data), 200


@compost_bins_bp.route('/', methods=['GET'])
def get_all_compost_bins():
    try:
        # Consulta la base de datos para obtener todos los compost bins
        compost_bins = CompostBin.query.all()
        app.logger.info(compost_bins)
        # Crea una lista para almacenar los datos de cada compost bin con su última medición
        compost_bins_data = []

        # Itera a través de los compost bins y obtén su última medición
        for compost_bin in compost_bins:
            last_measurement = Measurement.query.filter_by(compost_bin_id=compost_bin.compost_bin_id).order_by(
                Measurement.timestamp.desc()).first()
            compost_bin_data = {
                'id': compost_bin.compost_bin_id,
                'name': compost_bin.name,
                'last_measurement': {
                    'temperature': last_measurement.temperature,
                    'humidity': last_measurement.humidity,
                    'timestamp': last_measurement.timestamp.isoformat()
                }
            }
            compost_bins_data.append(compost_bin_data)

        # Devuelve los datos en formato JSON
        return jsonify(compost_bins_data)

    except Exception as e:
        return jsonify({'error': str(e)}), 500


#
@compost_bins_bp.route('/all_ids', methods=['GET'])
def get_all_compost_bin_ids():
    try:
        # Consulta la base de datos para obtener todos los compost bins
        compost_bins = CompostBin.query.all()

        # Obtén los IDs de todas las composteras
        compost_bin_ids = [compost_bin.compost_bin_id for compost_bin in compost_bins]

        # Devuelve los IDs en formato JSON
        return jsonify(compost_bin_ids)

    except Exception as e:
        return jsonify({'error': str(e)}), 500


@compost_bins_bp.route('/add_measurement', methods=['POST'])
def add_measurement():
    try:
        data = request.get_json()

        if 'id' not in data or 'temperatura' not in data or 'humedad' not in data or 'datetime' not in data:
            return jsonify({'error': 'Los campos id, temperatura, humedad y datetime son obligatorios'}), 400

        compost_bin = CompostBin.query.get(data['id'])
        if compost_bin is None:
            return jsonify({'error': 'No se encontró una compostera con el ID proporcionado'}), 404

        new_measurement = Measurement(
            temperature=data['temperatura'],
            humidity=data['humedad'],
            timestamp=data['datetime'],
            compost_bin=compost_bin
        )

        db.session.add(new_measurement)
        db.session.commit()

        return jsonify({'message': 'Medición agregada correctamente'}), 201

    except Exception as e:
        return jsonify({'error': str(e)}), 500
