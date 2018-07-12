# PointTracker

You can ask "what is the current position of each point" from series of events. These event only includes "down", "move", and "up" but no "stationary". This class will attempts to determine the current state without knowing ID of any point, but by it's previous position when moved.