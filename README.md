GDBStub
=======

Attempt at implementing a gbd server for CpS 310 at BJU

Requirements
  
·         Initial connect/handshake [C-level] -- Benjamin (FINISHED)
·		  Implement CPU methods in handshake -- Benjamin
·         Get/set general purpose registers [C] -- Daniel (FINISHED)
·         Get/set regions of memory [C] -- Daniel (FINISHED)
·         Single step/continue (execution faked at first, of course) [C] -- Benjamin /Daniel(FINISHED)
·         Load new binary image [B-level] -- ? Don't know what this is. please exp'
				gdb will use mov commands instead if you reply with an empty packet.
			
·         Set/clear software breakpoints [A-level] -- Daniel(FINISHED) /Benjamin
				1110,00010010, immed 19..8, 0111, immed 3..0	
					where immed is ignored by arm hardware but can be used by a debugger
					to store additional information about the breakpoint