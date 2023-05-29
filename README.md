# Link2Folder
- This lets you create a link to a specified folder on your local machine or on a network drive.
- Example: You can create a link to your shared drive on your homelab dashboard, so you can open the shares from your browser somewhat directly.
- Note: As far as I know (I was lazy to research), this only works for Windows

# Demo
https://github.com/Bocko/Link2Folder/assets/61477246/3ccb73ad-7edb-49ec-ba64-6422ea4342be

# How it works
The app uses the [Pluggable Protocol](https://learn.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/aa767916(v=vs.85)) to launch itself, which will then open specified folder.
With this you can [Register](https://learn.microsoft.com/en-us/previous-versions/windows/internet-explorer/ie-developer/platform-apis/aa767914(v=vs.85)?redirectedfrom=MSDN) specific URI Schemes into the registry.
To register these Schemes, administrator privileges are required, and the app will ask for them when setting it up.
After that, when a browser tries to navigate using the URI we registered, it will try and start the app.
The app will get the path to the folder from the browser as an argument and will attempt to open it after some clean up.

# Guide
## I. Place the executable in its final place

The app will register itself to the place where the registration was done, so it's important that it won't move after that.
If you decide to move the app, you need to run the setup again.

## II. Registering the URI (Setup)
To do this, you have two options:

### 1. Run the app from a terminal / cmd
Run the app from a terminal with the `setup` argument.
After running the app, it will ask for administrator permissions (this is needed to modify the registry).
When the admin privileges are granted, the app will add the URI Scheme to the Registry.
And you're done!

### 2. Create a shortcut
To do this, you need to create a shortcut to the app.
After creating the shortcut, you need to open its properties by right-clicking on the shortcut and selecting properties.
In the properties, you need to add ` setup` to the end of the target textbox.
If done correctly, now all you need to do is double click on the shortcut and it will run the app with the correct argument to start the setup.

## III. Creating links
With the setup done, now you can create your links!
Just take the path of something you want to link to paste it after this `link2folder:\\<your path here>`.
- Note: If you want to add links to html files, you need to put an additional backslash (\) after every backslash.
This is to let html know that it's just a backslash character and not a part of an element.
