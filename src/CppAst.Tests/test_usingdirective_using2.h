#ifndef testusing2_h
#define testusing2_h

namespace One {
namespace Two {
struct MyStruct;
using MyStructPtr = MyStruct*;
}
}

struct MyStruct3
{
	static void myFunc(One::Two::MyStructPtr x);
	static void myFunc2(const One::Two::MyStruct& x);
};

#endif