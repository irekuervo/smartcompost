# Crear la instancia de la aplicación Flask
from .application import app

# Importa los blueprints
from .views.compost_bins import compost_bins_bp
from .views.measurements import measurements_bp

# from app_pkg.views.compost_bins import compost_bins_bp
# from app_pkg.views.measurements import measurements_bp

# Suscribir los blueprints a la aplicación
app.register_blueprint(compost_bins_bp, url_prefix='/api/compost_bins')
app.register_blueprint(measurements_bp, url_prefix='/api/measurements')
