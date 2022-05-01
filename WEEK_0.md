# Basic ideas
![Week 0: system diagram](/img/Pasted%20image%2020220428153950.png)
The system is intended to be as symple as possible. Each task tracker dashboard state is managed in a single aggregate. The client, after authentication, issues commands to the `main` service that validates the command against the latest version of the ES log state, and persists resulting events to the log (one topic/stream per dashboard instance). The client gets back the version of the log resulting from the command which it can await for to display spinners, loading state, etc.

Other services can subscribe to the ES log (async communication) and fold it into the view that it's responsible for.

# Services
## Authorization service (`authN service`)
Managed service. Provides only authentication. Authorization is managed by the `main` service (`admin` user can change authorization of other users). It has to be done this way to keep the state in one place. The only writer to the log.
## Main service (`main`)
A web server processing incoming commands from the client. Maintains an in-memory latest version of the aggregate to improve speed. Only provides
## Accounting and analytics services
Web servers subscribed to the ES log and maintaining their respective views in-memory. The views may be evoked from memory based on request frequency.

# Scaling
`main` service is a singleton per dashboard with a fallback. Having only one instance of the writer makes an in-memory aggregate always (except for the moments when switching to the fallback) up to date thus making optimistics commits to the log always successful.

Read instances may scale horizontally depending on the demand.