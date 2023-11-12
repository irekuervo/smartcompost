from datetime import datetime

from .application import db


class CompostBin(db.Model):
    __tablename__ = 'compost_bins'

    compost_bin_id = db.Column(db.Integer, primary_key=True)
    name = db.Column(db.String(100), nullable=False)
    description = db.Column(db.Text)

    measurements = db.relationship('Measurement', backref='compost_bin', lazy=True)


class Measurement(db.Model):
    __tablename__ = 'measurements'

    measurement_id = db.Column(db.Integer, primary_key=True)
    temperature = db.Column(db.Float, nullable=False)
    humidity = db.Column(db.Float, nullable=False)
    timestamp = db.Column(db.DateTime, default=datetime.utcnow)

    compost_bin_id = db.Column(db.Integer, db.ForeignKey('compost_bins.compost_bin_id'), nullable=False)
