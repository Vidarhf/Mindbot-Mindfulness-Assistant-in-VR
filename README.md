# Intro 
This repository includes some of the assets used in my master thesis: _Interactive Machine Teaching of Conversational
Agents in VR, and Mindbot: a Mindfulness
Assistant in VR_

[Paper](Docs/Paper.pdf)

[Publication](http://urn.kb.se/resolve?urn=urn:nbn:se:umu:diva-191709)

#Interactive Machine Teaching Loop
[IMTloop](Docs/imtloop.PNG)
The paper's proposed Interactive machine teaching (IMT) loop for VR-assistants. 
The human expert acts as a teacher, engaging in tasks translating their knowledge to the machine learner via the interactive teaching environment. 

On updated knowledge from the teacher, the machine learner produces a new assistant model. Finally, assistant model is continuously deployed for interaction with the VR-application.

In this project, the ML agent and ITE (Rasa X) is built and hosted on a local web-server with Kubernetes container orchestration. Continuous Integration(CI), Deployment(CD), and testing pipeline was built as this enables integration of improvements made after a teaching session, and if it positively impacts the performance: deployment into production and subsequently the VR application.

#Activity Diagram
[activity](Docs/ActivityDiagram.PNG)
Activity flow of the user initiating dialogue with the assistant. 


The user triggers an action by pressing a button on the controller and speaking. 
The speech API then recognizes text from the recorded audio. 
This is then posted via to the assistant API which classifies the user's intent and provides an appropriate action-response. 
The assistants action-response may call for a game event (eg., to start a breathing exercise), the event is then executed by the game engine prior to the response text being synthesized into a voice clip played out for the user.

While this system deployed an assistant on a webserver and made api calls to the speech engine, This could've been done by integrating software development kits into the game engine and executed locally.
