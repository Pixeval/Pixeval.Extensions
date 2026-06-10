from pathlib import Path
import json

from setuptools import setup


metadata_file = Path(__file__).resolve().parents[1] / "Pixeval.Extensions.IDL" / "metadata.json"
metadata = json.loads(metadata_file.read_text(encoding="utf-8"))

setup(version=metadata["version"])
