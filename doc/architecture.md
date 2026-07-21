\# System Architecture



\## High-Level Overview

The application follows \*\*MVC + Service Layer\*\* pattern with clean separation of concerns.



\### Layers

\- \*\*Presentation\*\*: Razor Views + Controllers

\- \*\*Application\*\*: Services (CVGeneratorService, EmailSender)

\- \*\*Domain\*\*: Models + ViewModels

\- \*\*Infrastructure\*\*: Data (EF Core), Identity, External Services



\## Key Design Decisions

\- Dynamic attributes using `AttributeDefinition` + JSON storage

\- SignalR for real-time discussions

\- QuestPDF for server-side PDF generation

