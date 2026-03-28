# Projektdokumentation Connectivity Challenge: Connection Platform

## 1. Die Vision

Das Connection Platform ist eine hybride Networking-Plattform, die die Lücke zwischen physischen und virtuellen Event-Teilnehmern schliesst. Anstatt auf statische "Profillisten" oder starre "Meeting-Räume" zu setzen, nutzt DSC **Context-First Networking**. Das System organisiert Nutzer in sich entwickelnde "Interessengruppen" (Clusters) basierend auf Echtzeit-Themen. Dadurch fühlt sich digitale Interaktion so natürlich und ungezwungen an wie ein Gespräch auf einer echten Messe oder Party.

## 2. Die Architektur (Ein Blick "unter die Haube")

Das System basiert auf einer **modularen Mikroservice-Architektur**, die vollständig mit **Docker** containerisiert wurde, um eine einfache Skalierbarkeit zu gewährleisten.

### **A. Core Backend (.NET Core + MariaDB)**

-   **Rolle:** Die "Source of Truth" und der Traffic-Controller des Systems.
    
-   **Funktionalität:** Verwaltet die Benutzer-Authentifizierung (JWT), das Raum-Management und die Team-Planung.
    
-   **Datenhaltung:** Nutzt einen MariaDB-Container (`comm_db`) zur Speicherung von Benutzerdaten und persistenten Raumzuständen.
    
-   **Sicherheit:** Implementiert ein striktes JWT-basiertes Authentifizierungssystem mit einem gemeinsam genutzten Secret über die gesamte Umgebung hinweg.
    

### **B. KI-Analyse-Service (Cloud-API Integration)**

-   **Rolle:** Die "thematische Intelligenz".
    
-   **Funktionalität:** Analysiert Raumbeschreibungen und Metadaten über eine Cloud-Schnittstelle, um "Makro-Themen" zu identifizieren.
    
-   **Dynamik:** Die KI erkennt Ähnlichkeiten zwischen Gesprächen und liefert die Daten, um Räume mit verwandten Themen visuell zu gruppieren.
    

### **C. Das Frontend (Vanilla Canvas UI)**

-   **Rolle:** Die visuelle Landkarte.
    
-   **Implementierung:** Ein leichtgewichtiger Ansatz mit HTML5 Canvas (kein schwerfälliges SPA-Framework).
    
-   **Visualisierung:** Räume werden nicht als Liste, sondern als **Knoten (Nodes)** auf einer 2D-Fläche dargestellt. Cluster bilden sich visuell: Räume mit ähnlichen Themen "gravitieren" zueinander, sodass Nutzer die "Hotspots" interessanter Diskussionen sofort erkennen können.
    

----------

## 3. Die Innovation: Warum dieses System?

-   **Hürden abbauen (Der "Lurk"-Faktor):** Im Gegensatz zu Zoom oder Teams, wo der Beitritt oft wie eine Verpflichtung wirkt, erlaubt die Canvas-UI den Nutzern, Themen zu sondieren, bevor sie Kamera oder Mikrofon aktivieren.
    
-   **Physisch-Virtuelle Brücke:** Physische Teilnehmer können per QR-Code eine "Makro-Ansicht" der digitalen Welt auf ihrem Handy öffnen. So finden sie sofort die relevantesten virtuellen Cluster, ohne sich durch Menüs suchen zu müssen.
    
-   **Adaptives UI:** Das System hostet nicht nur Gespräche, es kartografiert sie. Wenn sich Gesprächsthemen ändern, morphen die Cluster auf der Karte mit. Das sorgt dafür, dass die relevantesten Themen immer am sichtbarsten sind.
    

----------

## 4. LiveKit-Integration (Geplante Videokommunikation)

Die Architektur ist darauf vorbereitet, Video-Streaming nahtlos zu integrieren:

1.  **Auswahl:** Ein Nutzer klickt auf einen "Knoten" auf dem Canvas.
    
2.  **Auth:** Das Frontend ruft `/rooms/join` auf. Das Backend stellt einen signierten **LiveKit-JWT** aus.
    
3.  **Stream:** Das Frontend nutzt das LiveKit-JS-SDK, um sich mit diesem Token direkt mit dem LiveKit-Server zu verbinden. Der "Knoten" wird so zum aktiven Video-Feed.
    

----------

## 5. Technische Bereitstellung (Entwickler-Notizen)

-   **Netzwerk:** Alle Dienste kommunizieren über ein benutzerdefiniertes Docker-Bridge-Netzwerk (`my-app-net`).
    
-   **Service Discovery:** Die Dienste erreichen sich gegenseitig über Containernamen (z. B. `http://mariadb-local:3306` oder `http://CommBackend:8080`).
    
-   **Umgebung:** Die Entwicklung wird durch `ASPNETCORE_ENVIRONMENT=Development` unterstützt, was Swagger-Dokumentation und Echtzeit-Debugging ermöglicht.
    

----------

### Ein abschliessender Gedanke zum Design

Der Clou am Canvas-System ist die **psychologische Nähe**. Da Menschen Entscheidungen oft nach Gefühl und Beschreibungen treffen, spiegelt die räumliche Nähe der Knoten auf dem Bildschirm die inhaltliche Nähe der Themen wider. Wir bauen keinen Entscheidungsbaum für Computer, sondern ein digitales Ökosystem für Menschen.