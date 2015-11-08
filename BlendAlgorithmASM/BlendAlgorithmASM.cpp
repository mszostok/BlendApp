// BlendAlgorithmASM.cpp : Defines the exported functions for the DLL application.
//

#include "stdafx.h"
#include "BlendAlgorithmASM.h"


// This is an example of an exported variable
BLENDALGORITHMASM_API int nBlendAlgorithmASM=0;

// This is an example of an exported function.
BLENDALGORITHMASM_API int fnBlendAlgorithmASM(void)
{
	return 42;
}

// This is the constructor of a class that has been exported.
// see BlendAlgorithmASM.h for the class definition
CBlendAlgorithmASM::CBlendAlgorithmASM()
{
	return;
}
