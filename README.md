# Persistent-Virtual-Graffiti

**The Idea:**
Graffiti is an excellent form of self expression. Beyond that, it can be a beautiful genre of art. However, graffiti is often done in public places, which devalues property and is often seen as vandalism. Indeed, go through any city and you will see graffiti on buildings. Most of the time it is not the owner decorating their walls in the name of self expression.
So, I set out to create a solution.  This cross platform application provides a best of both worlds scenario for artists and citizens both. 
When the user opens the app, they are sent into an augmented reality. They see the world through their phones camera. When you look at flat surfaces, be it a wall, floor, ceiling, or table, the phone sends ray casts and detects a flat surface. Then, the user holds down a button on the app, and their phone is turned into a spray can. With the button pressed, the user can move their phone and paint will spray from their phone and stick to any flat surface. 
Then, when I open my app in the same room you created art in, I can view your wall through my phone, and your creation will appear. 

**How it works:**
Using Unity, I was able to create a cross platform augmented reality application. The phone sends ray casts to detect flat planes. Each time a plane is  detected, information about the plane is transmitted to MongoDB. When the user holds down a button, the app shoots paint out of the top of the phone. If this paint touches one of the detected planes, it sticks. After pushing publish, all of the information about the painting is also stored to MongoDB.

When a separate user opens the app, I get their location, and query the database for paintings in a similar area. That art is then displayed. 3D math is done to place that art in the appropriate location, although there is a lot of room for improvement in this department. 

**The Vision:**
The potential of this project is limitless. If I had unlimited time, I would spend it adding profiles, AI system to detect rooms, more realsitic paint, featured artist of the day, clues as to where to find the nearest art, the ability to like creations, and much more. I see this as a new frontier of social media. An artisitic form of social media where users can either be artistic, or just leave fun message for their friends. All of this, with the obvious benefit of the real world being untouched. A secret world exists for users of my app.

**Screenshots:**
![image](https://user-images.githubusercontent.com/49215523/153688008-90394e70-8845-473e-bf49-c9ea5bed8a2d.png)
![image](https://user-images.githubusercontent.com/49215523/153688014-4060b5ce-ccde-4b24-854b-077daed29139.png)
![image](https://user-images.githubusercontent.com/49215523/153688025-38fdb515-6b65-4414-b654-7ba28a8d741d.png)
![image](https://user-images.githubusercontent.com/49215523/153688034-30f34c80-17f2-4364-947f-5aa1227ebc91.png)
![image](https://user-images.githubusercontent.com/49215523/153688042-b7caf4dd-eb06-4c0b-b012-d06a7ed5c18e.png)
