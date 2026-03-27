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
#### POST /auth/login
```json
{
    "username" : "username",
    "password" : "password"
}
```

#### POST /auth/register
```json
{
    "username" : "username",
    "password" : "password"
}
```

#### POST /auth/refresh
*refreshes user*

#### GET /user
*gets all users*

#### GET /user/{id}
*gets single user with id*

