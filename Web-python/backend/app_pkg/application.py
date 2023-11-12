import logging
import os
from flask import Flask
from flask_cors import CORS
from flask_sqlalchemy import SQLAlchemy

app = Flask(__name__)
CORS(app)  # Esto habilita CORS para todas las rutas

# Accede a la variable de entorno DATABASE_URL
database_url = os.environ.get('DATABASE_URL')

# Configura SQLAlchemy con la URI de la base de datos
app.config['SQLALCHEMY_DATABASE_URI'] = database_url

# Configura otras variables de entorno si es necesario
# app_pkg.config['OTRA_VARIABLE'] = os.environ.get('OTRA_VARIABLE')

# Resto de la configuración
db = SQLAlchemy(app)

# Configuracion del logger
app.logger.setLevel(logging.INFO)  # Configura el nivel de registro
formatter = logging.Formatter('[%(asctime)s] [%(levelname)s] - %(message)s')

# Configura el destino de registro en un archivo
file_handler = logging.FileHandler('app_pkg.log')
file_handler.setFormatter(formatter)
app.logger.addHandler(file_handler)

# Configura el destino de registro en la consola
console_handler = logging.StreamHandler()
console_handler.setFormatter(formatter)
app.logger.addHandler(console_handler)
