# sentiment_api.py

from flask import Flask, request, jsonify
from transformers import pipeline

# Inicjalizacja Flask
app = Flask(__name__)

# Inicjalizacja gotowego pipeline'u do analizy sentymentu
sentiment_pipeline = pipeline("sentiment-analysis")

def normalize_label(result):
    label = result['label']
    score = result['score']

    # Prosty heurystyczny próg na 'neutral'
    if score < 0.6:
        return "neutral"
    return "positive" if label == "POSITIVE" else "negative"

@app.route('/predict', methods=['POST'])
def predict_sentiment():
    data = request.json
    text = data.get("message")

    if not text:
        return jsonify({"error": "Brak pola 'message'"}), 400

    result = sentiment_pipeline(text)[0]
    sentiment = normalize_label(result)

    return jsonify({
        "message": text,
        "sentiment": sentiment,
        "confidence": round(result['score'], 4)
    })

if __name__ == '__main__':
    app.run(debug=True, host="0.0.0.0", port=5000)
