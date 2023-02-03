using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace Mocha.Common;

public enum CVarFlags
{
	// Mirrors native

	None = 0,

	// If this isn't present, it's inherently assumed to be a variable
	Command = 1 << 0,

	// If this is present, it lives in managed space
	Managed = 1 << 1,

	// This cvar was created by the game, it should be wiped on hotload
	Game = 1 << 2,

	// Save this convar to cvars.json
	Archive = 1 << 3,

	Cheat = 1 << 4,

	Temp = 1 << 5,

	Replicated = 1 << 6,
}
