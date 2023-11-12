import os


class Config:
    SECRET_KEY = os.environ.get('SECRET_KEY') or 'tu_clave_secreta_predeterminada'
    SQLALCHEMY_DATABASE_URI = os.environ.get(
        'DATABASE_URL') or 'postgresql://usuario:contraseña@localhost/nombre_base_de_datos'
    SQLALCHEMY_TRACK_MODIFICATIONS = False
    DEBUG = False


class DevelopmentConfig(Config):
    DEBUG = True


class TestingConfig(Config):
    TESTING = True


class ProductionConfig(Config):
    DEBUG = False
