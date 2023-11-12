from .app_pkg.application import app

if __name__ == '__main__':
    app.run(host="0.0.0.0", port=8080)

# http://0.0.0.0:8080/api/compost_bins/compost_bins