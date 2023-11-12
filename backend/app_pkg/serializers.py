from marshmallow import Schema, fields


class MeasurementSchema(Schema):
    class Meta:
        ordered = True

    measurement_id = fields.Int()
    temperature = fields.Float()
    humidity = fields.Float()
    timestamp = fields.DateTime()


class CompostBinSchema(Schema):
    compost_bin_id = fields.Int()
    name = fields.String()
    description = fields.String()
    measurements = fields.Nested(MeasurementSchema, many=True)

