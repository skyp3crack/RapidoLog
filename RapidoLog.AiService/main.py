import os
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel,Field
from google import genai
from google.genai import types

app = FastAPI(title="RapidoLog AI Extraction Engine")

client = genai.Client()

class StructuredAddress(BaseModel): #define pydantic model class for address
    street: str = Field(description="The Street name,building block,floor, or house number.")
    city: str = Field(description = "The city name or town in Malaysia")
    postcode: str = Field(description = "The 5 digit Malaysian postcode number")
    state: str = Field(description = "The state name in Malaysia ( e.g.. Selangor, Johor, Pahang).")


class AddressRequest(BaseModel): #Define model to accept incoming JSON payload
    rawAddress: str

   

@app.post("/api/extract-address", response_model=StructuredAddress)
async def extract_address(request: AddressRequest):
    try:
        prompt = f"Extract the Malaysian delivery address details from this unstructured text: '{request.rawAddress}'"
        response = client.models.generate_content(
            model='gemini-2.5-flash',
            contents=prompt,
            config=types.GenerateContentConfig(
                response_mime_type="application/json",
                response_schema=StructuredAddress,
                temperature=0.1
            ),
        )
        
        # The response is guaranteed to match our Pydantic schema
        return StructuredAddress.model_validate_json(response.text)
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app,host="127.0.0.1", port=8000)
