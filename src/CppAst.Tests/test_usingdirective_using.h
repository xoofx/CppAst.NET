#ifndef testusing_h
#define testusing_h

namespace One {
namespace Two {
struct MyStruct;
using MyStructPtr = One::Two::MyStruct*;
}
}

struct MyStruct2
{
	One::Two::MyStructPtr x;
};

#endif