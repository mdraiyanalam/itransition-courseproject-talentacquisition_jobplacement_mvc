import requests
import json
from odoo import models, fields, api
from odoo.exceptions import UserError

class TalentPosition(models.Model):
    _name = 'talent.position'
    _description = 'TalentHub Position'
    _order = 'id desc'

    name = fields.Char(string='Position Title', required=True)
    company = fields.Char(string='Company')
    description = fields.Text(string='Description')
    project_tags = fields.Char(string='Project Tags')
    total_applications = fields.Integer(string='Total Applications', default=0)
    api_token_used = fields.Char(string='API Token Used', readonly=True)
    
    attribute_ids = fields.One2many(
        'talent.position.attribute',
        'position_id',
        string='Attributes'
    )

    def action_export_to_talenthub(self):
        self.ensure_one()

        api_url = "https://itransition-courseproject-f3vg.onrender.com/api/positions"
        api_key = "!a@i1W19q|HM"

        payload = {
            "Title": self.name or "",
            "Description": self.description or "",
            "Company": self.company or "",
            "ProjectTags": self.project_tags or "",
        }

        headers = {
            "Content-Type": "application/json",
            "X-Api-Key": api_key
        }

        try:
            response = requests.post(
                api_url,
                data=json.dumps(payload),
                headers=headers,
                timeout=20
            )

            if response.status_code in (200, 201):
                result = response.json()
                raise UserError(
                    f"Successfully exported to TalentAcqusition!\n\n"
                    f"New Position ID: {result.get('id')}\n"
                    f"Message: {result.get('message')}"
                )
            else:
                raise UserError(
                    f"Export failed (Status {response.status_code}):\n\n{response.text}"
                )

        except requests.exceptions.RequestException as e:
            raise UserError(f"Connection error:\n{str(e)}")
        except Exception as e:
            raise UserError(f"Unexpected error:\n{str(e)}")