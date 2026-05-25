import os
from fastapi import FastAPI, HTTPException
from pydantic import BaseModel,Field
from google import genai
from google.genai import types
from dotenv import load_dotenv

load_dotenv()
app = FastAPI(title="RapidoLog AI Extraction Engine")

client = genai.Client(api_key=os.getenv("GEMINI_API_KEY"))

class StructuredAddress(BaseModel): #define pydantic model class for address
    street: str = Field(description="The Street name,building block,floor, or house number.")
    city: str = Field(description = "The city name or town in Malaysia")
    postcode: str = Field(description = "The 5 digit Malaysian postcode number")
    state: str = Field(description = "The state name in Malaysia ( e.g.. Selangor, Johor, Pahang).")


class AddressRequest(BaseModel): #Define model to accept incoming JSON payload
    model_config = {"populate_by_name": True}
    raw_address: str = Field(alias="rawAddress")

   

@app.post("/api/extract-address", response_model=StructuredAddress)
async def extract_address(request: AddressRequest):
    try:
        prompt = f"""
        You are an expert Malaysian logistics data parser. 
        Extract the core address components from this unstructured text: '{request.raw_address}'
        
        CRITICAL RULES:
        1. Remove ALL conversational filler, slang, and instructions (e.g., 'hantar ke', 'tolong ek', 'dekat', 'bro', 'tq').
        2. Clean and format the street, building, apartment, or unit details nicely.
        3. Extract the accurate 5-digit postcode, city, and state.
        """
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
        print(response.text)
        return StructuredAddress.model_validate_json(response.text)
        
    except Exception as e:
        raise HTTPException(status_code=500, detail=str(e))


if __name__ == "__main__":
    import uvicorn
    uvicorn.run(app,host="127.0.0.1", port=8000)
