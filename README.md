GDBStub
=======

Attempt at implementing a gbd server for CpS 310 at BJU

Requirements
  
·         Initial connect/handshake [C-level]
·         Get/set general purpose registers [C]
·         Get/set regions of memory [C]
·         Single step/continue (execution faked at first, of course) [C]
·         Load new binary image [B-level]
·         Set/clear software breakpoints [A-level]