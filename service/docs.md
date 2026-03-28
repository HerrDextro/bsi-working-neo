# Service Endpoints

## Overview
This service exposes HTTP endpoints for monitoring health and dynamically updating a meeting topic based on recent conversation transcripts.

---

## Endpoints

### `GET /health`

**Description**  
Simple health check endpoint to verify that the service is running.

**Response**
- `200 OK`
- Body:
  ```json
  "Service is healthy"
### `POST /rooms/{roomId}/update-topic`

**Description**  
Processes the latest segment of a meeting transcript and generates a concise topic summary using an AI-powered topic extraction service.

This allows participants (or UI clients) to quickly understand what the current discussion in a meeting is about.

----------

## Request

### Path Parameters

-   `roomId` (string)  
    Identifier of the meeting room.

### Body

{  
 "text": "string"  
}

-   `text`: The most recent portion of the meeting transcript (e.g., last ~1 minute of conversation).

----------

## Processing Flow

1.  The endpoint receives the transcript segment.
2.  It sends the text to `TopicExtractorService`.
3.  The service uses an LLM (via Groq) to:
    -   Summarize the conversation
    -   Extract the main discussion topic
    -   Detect whether the conversation represents a branch from the previous topic
4.  (Planned) The extracted topic will be stored in LiveKit room metadata.
5.  The result is returned to the client.

----------

## Response

**Status:** `200 OK`

{  
 "room": "string",  
 "topic": "string",  
 "isBranch": true,  
 "debug_llm_said": "string"  
}

### Fields

-   `room`  
    The room identifier.
-   `topic`  
    AI-generated summary of the current discussion topic.
-   `isBranch`  
    Indicates whether the topic represents a shift or branch from the previous discussion.
-   `debug_llm_said`  
    Raw, unsanitized LLM response (useful for debugging, mildly dangerous if exposed publicly).