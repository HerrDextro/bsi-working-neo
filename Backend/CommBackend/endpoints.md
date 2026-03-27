## Calls
#### GET /rooms
*returns a list of all active calls*

#### POST /rooms/create
```roomName : name```

#### POST /rooms/join
```roomName : name```

#### DELETE /rooms/{name}
*deletes room, 404 if not exists*

#### PATCH /rooms/{name}
```newTitle : name```
*updates room name*

## Auth
