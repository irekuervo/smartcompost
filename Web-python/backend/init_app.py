import random
from datetime import datetime

from app_pkg.application import app
from app_pkg.application import db
from app_pkg.models import CompostBin, Measurement

if __name__ == '__main__':
    with app.app_context():
        # Crea la base de datos
        db.create_all()

        # Aplica migraciones
        db.session.commit()  # Asegúrate de que las migraciones se apliquen antes de agregar datos

        # Crea dos composteras
        compost_bin1 = CompostBin(name='Compost Bin 1')
        compost_bin2 = CompostBin(name='Compost Bin 2')

        db.session.add(compost_bin1)
        db.session.add(compost_bin2)
        db.session.commit()

        # Agrega mediciones para compost_bin1
        for _ in range(10):
            temperature = round(random.uniform(5, 50), 2)
            humidity = round(random.uniform(60, 100), 2)
            timestamp = datetime.utcnow()

            measurement = Measurement(
                temperature=temperature,
                humidity=humidity,
                timestamp=timestamp,
                compost_bin=compost_bin1
            )
            db.session.add(measurement)

        # Agrega mediciones para compost_bin2
        for _ in range(10):
            temperature = round(random.uniform(5, 50), 2)
            humidity = round(random.uniform(60, 100), 2)
            timestamp = datetime.utcnow()

            measurement = Measurement(
                temperature=temperature,
                humidity=humidity,
                timestamp=timestamp,
                compost_bin=compost_bin2
            )
            db.session.add(measurement)

        db.session.commit()

    app.run(host="0.0.0.0", port=8080)
