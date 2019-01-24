# Platform

## Build and test the application

### Database

If Docker is installed, run the command 
```docker run -d -p 27017:27017 mongo```

### The Application

Run the `build.bat` script in order to restore, build and test (if you've selected to include tests) the application:

```
> ./build.bat
```

Then navigate to localhost:8765 in a browser