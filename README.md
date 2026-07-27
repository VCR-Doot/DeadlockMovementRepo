# Deadlock Movement API

An API library for survivors based on Deadlock Heroes to utilize Deadlock's movement system all in one place!



## Utilization

Survivors using this as a hard dependency can change their survivor's main to the included "DLMain" CharacterMain state, or create an inheritor of said state.

## NOTE

Due to a known bug regarding sprint input the survivor should also have the SprintAnyDirection body flag to allow omnidirectional sprint.

## Animation hooks

Currently all custom states utilize animations on the "FullBody, Override" layer. This is planned to be updated to it's own layer for certain animations (namely Sliding) to allow for other layers to still function (such as using skills during slide). The following are the names and layers for each movement animation call
\* "FullBody, Override", "Slide"
\* "FullBody, Override", "Dash"
\* "FullBody, Override", "DashJump"



Sliding specifically uses the isSliding param due to no set duration, so add that as the transition condition in and out of the slide state

