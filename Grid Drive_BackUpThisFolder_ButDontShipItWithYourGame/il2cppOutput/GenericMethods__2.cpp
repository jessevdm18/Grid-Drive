#include "pch-cpp.hpp"





template <typename R>
struct VirtualFuncInvoker0
{
	typedef R (*Func)(void*,const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_virtual_invoke_data(slot, obj);
		return ((Func)invokeData.methodPtr)(obj,invokeData.method);
	}
};
template <typename R>
struct InterfaceFuncInvoker0
{
	typedef R (*Func)(void*,const RuntimeMethod*);

	static inline R Invoke (Il2CppMethodSlot slot, RuntimeClass* declaringInterface, RuntimeObject* obj)
	{
		const VirtualInvokeData& invokeData = il2cpp_codegen_get_interface_invoke_data(slot, obj, declaringInterface);
		return ((Func)invokeData.methodPtr)(obj,invokeData.method);
	}
};
template <typename T1>
struct InvokerActionInvoker1;
template <typename T1>
struct InvokerActionInvoker1<T1*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1)
	{
		void* params[1] = { p1 };
		method->invoker_method(methodPtr, method, obj, params, params[0]);
	}
};
template <typename T1, typename T2>
struct InvokerActionInvoker2;
template <typename T1, typename T2>
struct InvokerActionInvoker2<T1, T2*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1 p1, T2* p2)
	{
		void* params[2] = { &p1, p2 };
		method->invoker_method(methodPtr, method, obj, params, params[1]);
	}
};
template <typename T1, typename T2>
struct InvokerActionInvoker2<T1*, T2*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1, T2* p2)
	{
		void* params[2] = { p1, p2 };
		method->invoker_method(methodPtr, method, obj, params, params[1]);
	}
};
template <typename T1, typename T2, typename T3>
struct InvokerActionInvoker3;
template <typename T1, typename T2, typename T3>
struct InvokerActionInvoker3<T1, T2*, T3*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1 p1, T2* p2, T3* p3)
	{
		void* params[3] = { &p1, p2, p3 };
		method->invoker_method(methodPtr, method, obj, params, params[2]);
	}
};
template <typename T1, typename T2, typename T3>
struct InvokerActionInvoker3<T1*, T2*, T3*>
{
	static inline void Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1, T2* p2, T3* p3)
	{
		void* params[3] = { p1, p2, p3 };
		method->invoker_method(methodPtr, method, obj, params, params[2]);
	}
};
template <typename R, typename T1, typename T2>
struct InvokerFuncInvoker2;
template <typename R, typename T1, typename T2>
struct InvokerFuncInvoker2<R, T1, T2*>
{
	static inline R Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1 p1, T2* p2)
	{
		R ret;
		void* params[2] = { &p1, p2 };
		method->invoker_method(methodPtr, method, obj, params, &ret);
		return ret;
	}
};
template <typename R, typename T1, typename T2>
struct InvokerFuncInvoker2<R, T1*, T2*>
{
	static inline R Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1, T2* p2)
	{
		R ret;
		void* params[2] = { p1, p2 };
		method->invoker_method(methodPtr, method, obj, params, &ret);
		return ret;
	}
};
template <typename R, typename T1, typename T2, typename T3, typename T4, typename T5>
struct InvokerFuncInvoker5;
template <typename R, typename T1, typename T2, typename T3, typename T4, typename T5>
struct InvokerFuncInvoker5<R, T1*, T2, T3, T4*, T5*>
{
	static inline R Invoke (Il2CppMethodPointer methodPtr, const RuntimeMethod* method, void* obj, T1* p1, T2 p2, T3 p3, T4* p4, T5* p5)
	{
		R ret;
		void* params[5] = { p1, &p2, &p3, p4, p5 };
		method->invoker_method(methodPtr, method, obj, params, &ret);
		return ret;
	}
};

struct ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28;
struct Dictionary_2_t73D7B210F00FD83B4F194279B052C32E9FCB1D04;
struct Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B;
struct Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29;
struct Dictionary_2_t36B17C1CDA9BE4EE8C94D25A0DC9A7F4C75D51D7;
struct Dictionary_2_tD81F54C87D78FE70A5DE7DAA170AE5EB4E54E8C3;
struct Dictionary_2_t070EAA8A0D7DC2B4DA1223E3809A83B3933BF21A;
struct Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415;
struct Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053;
struct Dictionary_2_tB3BF685342B8A1AB5995EE46B7014CBC0B281D5C;
struct EventHandler_1_tF2D41B212D800E7E7D00F9BDEA817E57153988BF;
struct EventHandler_1_t9A81151178F9BE57BCBDEC74D73D78671B00EC6E;
struct Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457;
struct Func_1_tBB8824FA8746333BFFF3AB3CE4A41B58450AF431;
struct GPUBuffer_1_tB27A42EE52EA38FD7117C4EC0B1E42703FFE4C36;
struct IComparer_1_tF8C7AEFAE0CBF98861929681D75DB2435790296B;
struct IComparer_1_t7E870201BF4F9A2658830A9FA35ED97BC476A8B4;
struct IComparer_1_tCA720E7837F12C38065A4AB632B5FAAD19EC30AA;
struct IComparer_1_t01B466D62BD708A8A8BBAB6085E75DCF7678A908;
struct IEqualityComparer_1_tDBFC8496F14612776AF930DBF84AFE7D06D1F0E9;
struct IEqualityComparer_1_tAE94C8F24AD5B94D4EE85CA9FC59E3409D41CAF7;
struct IList_1_t09217E1EDF7CCAED72B667F406AAC92AB64B8790;
struct IList_1_t1E95A05AFA88C09BA96A40B431F7D5867BD0D622;
struct IList_1_t7DC7A22F42EEFA1E8D796516A9F22282DF4EB5FA;
struct IList_1_t8AC59FFD0F90EC2DAD4C3FA39B2A5851F9D2987A;
struct KeyCollection_t85B2525EEF5D5A84F3F738D01E769871A502C0E0;
struct KeyCollection_t34C61D137A6BA17F802C66A99B4F907FB7B205B7;
struct KeyCollection_t9DE92FC732B61C5A1326EA273DEA850C60DF053B;
struct KeyCollection_t404DBD619DDFFEB7189DE722452ED38D0B97B925;
struct ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050;
struct ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513;
struct ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263;
struct ReadOnlyCollection_1_t5B7AA4E006906DE6818A44873F2D5987EFBF3AB8;
struct ValueCollection_t4DC0B57BDC839D7601C76A4DD0BA01E29A9171B2;
struct ValueCollection_t9C5E3FCB79B0826DF5BA4112E9DA522A7293EF90;
struct ValueCollection_t24E99C3420B8E91B7B658FD1C070EBDABAC21373;
struct ValueCollection_t1E6646924FB29761CB665003BAACABAA4486908B;
struct CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466;
struct EntryU5BU5D_t6ED1C87FCA884F3F7E9D28BC7B09077E3C0AC763;
struct EntryU5BU5D_t774C3280E33B5E7DCE2AFFCC6CEE5EE54DA84998;
struct EntryU5BU5D_tA600D14CC543727DB85B5E846217C0A08FF66259;
struct EntryU5BU5D_tB21348395CEA13AD41732FA6EE2DDF664B06BFB7;
struct ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031;
struct CharU5BU5D_t799905CF001DD5F13F7DBB310181FC4D8B7D0AAB;
struct CustomAttributeNamedArgumentU5BU5D_tC0A39D9401E28662213F5958EFF5D26D0681B440;
struct CustomAttributeTypedArgumentU5BU5D_t6CAA09EC6AACBED57FC8B02C587D50BF6B738C6B;
struct DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771;
struct Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C;
struct IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF;
struct TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB;
struct UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299;
struct __CanonU5BU5D_tFF96AE6C231BB36A6CEE54CEEB72ED8E90201979;
struct __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC;
struct SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B;
struct Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07;
struct AndroidImpl_t09BB72854905028A1DF3FBA8772392723D2CCD76;
struct AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03;
struct AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0;
struct AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D;
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263;
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129;
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F;
struct Assembly_t;
struct AsyncCallback_t7FEF460CBDCFB9C5FA2EF776984778B9A4145F4C;
struct Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235;
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3;
struct Delegate_t;
struct DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E;
struct Dispatcher_tBD1370511B5D6C10B211FBD3AF8E4F9D6B3CD8C6;
struct EventHandler_tC6323FD7E6163F965259C33D72612C0E5B9BAB82;
struct Exception_t;
struct FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25;
struct FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2;
struct FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40;
struct FirebaseMonoBehaviour_t0DC02A14DFF90538B38698592F07B4373DB03C72;
struct GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8;
struct IAsyncResult_t7B9B5A0ECB35DCEC31B8A8122C37D687369253B5;
struct IDictionary_t6D03155AF1FA9083817AA5B6AD7DEEACC26AB220;
struct IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228;
struct IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391;
struct MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553;
struct MemberInfo_t;
struct MethodInfo_t;
struct SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6;
struct String_t;
struct Type_t;
struct Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E;
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915;
struct TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA;

IL2CPP_EXTERN_C RuntimeClass* AllocatorManager_tFB15A22029C8159A3DCD4C08935BE57D3E6B3C2C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* BurstCompiler_t2715484E1FF256726FC4D4D8E17C35A4C8DFA2B8_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Exception_t_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C RuntimeClass* _AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var;
IL2CPP_EXTERN_C String_t* _stringLiteral1D86DE1FC0445C55F712ED0F6D3E32FED0A1E724;
IL2CPP_EXTERN_C String_t* _stringLiteral2B6D6F48C27C60C3B55391AB377D9DC8F5639AA1;
IL2CPP_EXTERN_C String_t* _stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE;
IL2CPP_EXTERN_C String_t* _stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D;
IL2CPP_EXTERN_C String_t* _stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92;
IL2CPP_EXTERN_C String_t* _stringLiteral50E02893F5B6939534A49C535241D6B7BAFCF599;
IL2CPP_EXTERN_C String_t* _stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8;
IL2CPP_EXTERN_C String_t* _stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED;
IL2CPP_EXTERN_C String_t* _stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E;
IL2CPP_EXTERN_C String_t* _stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D;
IL2CPP_EXTERN_C String_t* _stringLiteralDC955F06F1DA8A3B06C4755211822B06D698CF11;
IL2CPP_EXTERN_C String_t* _stringLiteralE8744A8B8BD390EB66CA0CAE2376C973E6904FFB;
IL2CPP_EXTERN_C const RuntimeMethod* BurstCompiler_CompileFunctionPointer_TisTryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA_m24AC0F77E9E5A8C61B911FBAF3DC753E0B1FB4DC_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* FunctionPointer_1__ctor_mA6464FB1EEC3C76906932127ADC88D71257A9CB6_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeMethod* Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var;
IL2CPP_EXTERN_C const RuntimeType* AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_0_0_0_var;
struct Assembly_t_marshaled_com;
struct Delegate_t_marshaled_com;
struct Delegate_t_marshaled_pinvoke;
struct Exception_t_marshaled_com;
struct Exception_t_marshaled_pinvoke;

struct CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466;
struct CustomAttributeNamedArgumentU5BU5D_tC0A39D9401E28662213F5958EFF5D26D0681B440;
struct CustomAttributeTypedArgumentU5BU5D_t6CAA09EC6AACBED57FC8B02C587D50BF6B738C6B;
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918;
struct UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299;
struct __CanonU5BU5D_tFF96AE6C231BB36A6CEE54CEEB72ED8E90201979;
struct __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC;
struct SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B;

IL2CPP_EXTERN_C_BEGIN
IL2CPP_EXTERN_C_END

#ifdef __clang__
#pragma clang diagnostic push
#pragma clang diagnostic ignored "-Winvalid-offsetof"
#pragma clang diagnostic ignored "-Wunused-variable"
#endif
struct AllocatorCache_1_tE4A3FB98A13F6767AD7FC224210A0475AE15E18E  : public RuntimeObject
{
};
struct ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28  : public RuntimeObject
{
};
struct Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B  : public RuntimeObject
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____buckets;
	EntryU5BU5D_t6ED1C87FCA884F3F7E9D28BC7B09077E3C0AC763* ____entries;
	int32_t ____count;
	int32_t ____freeList;
	int32_t ____freeCount;
	int32_t ____version;
	RuntimeObject* ____comparer;
	KeyCollection_t85B2525EEF5D5A84F3F738D01E769871A502C0E0* ____keys;
	ValueCollection_t4DC0B57BDC839D7601C76A4DD0BA01E29A9171B2* ____values;
	RuntimeObject* ____syncRoot;
};
struct Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29  : public RuntimeObject
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____buckets;
	EntryU5BU5D_t774C3280E33B5E7DCE2AFFCC6CEE5EE54DA84998* ____entries;
	int32_t ____count;
	int32_t ____freeList;
	int32_t ____freeCount;
	int32_t ____version;
	RuntimeObject* ____comparer;
	KeyCollection_t34C61D137A6BA17F802C66A99B4F907FB7B205B7* ____keys;
	ValueCollection_t9C5E3FCB79B0826DF5BA4112E9DA522A7293EF90* ____values;
	RuntimeObject* ____syncRoot;
};
struct Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415  : public RuntimeObject
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____buckets;
	EntryU5BU5D_tA600D14CC543727DB85B5E846217C0A08FF66259* ____entries;
	int32_t ____count;
	int32_t ____freeList;
	int32_t ____freeCount;
	int32_t ____version;
	RuntimeObject* ____comparer;
	KeyCollection_t9DE92FC732B61C5A1326EA273DEA850C60DF053B* ____keys;
	ValueCollection_t24E99C3420B8E91B7B658FD1C070EBDABAC21373* ____values;
	RuntimeObject* ____syncRoot;
};
struct Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053  : public RuntimeObject
{
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ____buckets;
	EntryU5BU5D_tB21348395CEA13AD41732FA6EE2DDF664B06BFB7* ____entries;
	int32_t ____count;
	int32_t ____freeList;
	int32_t ____freeCount;
	int32_t ____version;
	RuntimeObject* ____comparer;
	KeyCollection_t404DBD619DDFFEB7189DE722452ED38D0B97B925* ____keys;
	ValueCollection_t1E6646924FB29761CB665003BAACABAA4486908B* ____values;
	RuntimeObject* ____syncRoot;
};
struct ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050  : public RuntimeObject
{
	RuntimeObject* ___list;
	RuntimeObject* ____syncRoot;
};
struct ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513  : public RuntimeObject
{
	RuntimeObject* ___list;
	RuntimeObject* ____syncRoot;
};
struct ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263  : public RuntimeObject
{
	RuntimeObject* ___list;
	RuntimeObject* ____syncRoot;
};
struct ReadOnlyCollection_1_t5B7AA4E006906DE6818A44873F2D5987EFBF3AB8  : public RuntimeObject
{
	RuntimeObject* ___list;
	RuntimeObject* ____syncRoot;
};
struct AllocatorManager_tFB15A22029C8159A3DCD4C08935BE57D3E6B3C2C  : public RuntimeObject
{
};
struct AndroidJNI_t531BC9A6383F7C0F76A1270297952462F52308EE  : public RuntimeObject
{
};
struct AndroidJNIHelper_t2C1AB9F6B2295C20B24108936A003F65F02D71DD  : public RuntimeObject
{
};
struct AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0  : public RuntimeObject
{
	GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* ___m_jobject;
	GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* ___m_jclass;
};
struct AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48  : public RuntimeObject
{
};
struct Assembly_t  : public RuntimeObject
{
};
struct Assembly_t_marshaled_pinvoke
{
};
struct Assembly_t_marshaled_com
{
};
struct FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586  : public RuntimeObject
{
	bool ___U3CIsPlayModeU3Ek__BackingField;
	EventHandler_1_tF2D41B212D800E7E7D00F9BDEA817E57153988BF* ___Updated;
	Action_tD00B0A84D7945E50C2DFFC28EFEE6ED44ED2AD07* ___UpdatedEventWrapper;
	EventHandler_1_t9A81151178F9BE57BCBDEC74D73D78671B00EC6E* ___ApplicationFocusChanged;
};
struct Impl_t9BC9F6C5466C4F180F0FE9B6736ED9B2354D87DF  : public RuntimeObject
{
};
struct MemberInfo_t  : public RuntimeObject
{
};
struct String_t  : public RuntimeObject
{
	int32_t ____stringLength;
	Il2CppChar ____firstChar;
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F  : public RuntimeObject
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_pinvoke
{
};
struct ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F_marshaled_com
{
};
struct IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F  : public RuntimeObject
{
};
struct IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C  : public RuntimeObject
{
};
struct NativeSlice_1_t135E2A23AA4EBDBCA846942F90F29A5004E8ACB5 
{
	uint8_t* ___m_Buffer;
	int32_t ___m_Stride;
	int32_t ___m_Length;
};
struct SharedStatic_1_t93EB5AFD7E0C5BF92AC0053F6F64C16421DCA08C 
{
	void* ____buffer;
};
struct AndroidImpl_t09BB72854905028A1DF3FBA8772392723D2CCD76  : public Impl_t9BC9F6C5466C4F180F0FE9B6736ED9B2354D87DF
{
	FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40* ___crashlyticsInternal;
	FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25* ___firebaseApp;
};
struct AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03  : public AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0
{
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22 
{
	bool ___m_value;
};
struct Byte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3 
{
	uint8_t ___m_value;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17 
{
	Il2CppChar ___m_value;
};
struct CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F 
{
	Type_t* ___U3CArgumentTypeU3Ek__BackingField;
	RuntimeObject* ___U3CValueU3Ek__BackingField;
};
struct CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F_marshaled_pinvoke
{
	Type_t* ___U3CArgumentTypeU3Ek__BackingField;
	Il2CppIUnknown* ___U3CValueU3Ek__BackingField;
};
struct CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F_marshaled_com
{
	Type_t* ___U3CArgumentTypeU3Ek__BackingField;
	Il2CppIUnknown* ___U3CValueU3Ek__BackingField;
};
struct Double_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F 
{
	double ___m_value;
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2  : public ValueType_t6D9B272BD21782F0A9A14F2E41F85A50E97A986F
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_pinvoke
{
};
struct Enum_t2A1A94B24E3B776EEF4E5E485E290BB9D4D072E2_marshaled_com
{
};
struct Int16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175 
{
	int16_t ___m_value;
};
struct Int32_t680FF22E76F6EFAD4375103CBBFFA0421349384C 
{
	int32_t ___m_value;
};
struct Int64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3 
{
	int64_t ___m_value;
};
struct IntPtr_t 
{
	void* ___m_value;
};
struct Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 
{
	int64_t ___f0;
	int64_t ___f1;
	int64_t ___f2;
	int64_t ___f3;
	int64_t ___f4;
	int64_t ___f5;
	int64_t ___f6;
	int64_t ___f7;
};
struct SByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5 
{
	int8_t ___m_value;
};
struct Single_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C 
{
	float ___m_value;
};
struct Spinner_t9606E334089E448EA313C31DC5EFC9345A58BCEB 
{
	int32_t ___m_Lock;
};
struct UInt16_tF4C148C876015C212FD72652D0B6ED8CC247A455 
{
	uint16_t ___m_value;
};
struct UInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF 
{
	uint64_t ___m_value;
};
struct Void_t4861ACF8F4594C3437BB48B6E56783494B843915 
{
	union
	{
		struct
		{
		};
		uint8_t Void_t4861ACF8F4594C3437BB48B6E56783494B843915__padding[1];
	};
};
struct AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 
{
	uint16_t ___Index;
	uint16_t ___Version;
};
struct SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 
{
	String_t* ___AssemblyName;
	Assembly_t* ___Assembly;
	Int32U5BU5D_t19C97395396A72ECAF310612F0760F165060314C* ___Dependencies;
};
struct SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53_marshaled_pinvoke
{
	char* ___AssemblyName;
	Assembly_t_marshaled_pinvoke ___Assembly;
	Il2CppSafeArray* ___Dependencies;
};
struct SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53_marshaled_com
{
	Il2CppChar* ___AssemblyName;
	Assembly_t_marshaled_com* ___Assembly;
	Il2CppSafeArray* ___Dependencies;
};
struct ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC 
{
	intptr_t ____value;
};
struct CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 
{
	void* ___srcCpuBuffer;
	GPUBuffer_1_tB27A42EE52EA38FD7117C4EC0B1E42703FFE4C36* ___dstGpuBuffer;
	NativeSlice_1_t135E2A23AA4EBDBCA846942F90F29A5004E8ACB5 ___pendingGpuCopies;
};
struct FunctionPointer_1_t57A2ADDF748E323E76FF7DAD737F07BA9549251E 
{
	intptr_t ____ptr;
};
struct FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C 
{
	intptr_t ____ptr;
};
struct UnmanagedArray_1_t7A336330780E05C924BB57ED337DDD2A66701FAB 
{
	intptr_t ___m_pointer;
	int32_t ___m_length;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___m_allocator;
};
struct Allocator_t996642592271AAD9EE688F142741D512C07B5824 
{
	int32_t ___value__;
};
struct AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D  : public RuntimeObject
{
	AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* ___javaInterface;
	intptr_t ___proxyObject;
};
struct CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02 
{
	CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F ___U3CTypedValueU3Ek__BackingField;
	bool ___U3CIsFieldU3Ek__BackingField;
	String_t* ___U3CMemberNameU3Ek__BackingField;
	Type_t* ____attributeType;
	MemberInfo_t* ____lazyMemberInfo;
};
struct CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02_marshaled_pinvoke
{
	CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F_marshaled_pinvoke ___U3CTypedValueU3Ek__BackingField;
	int32_t ___U3CIsFieldU3Ek__BackingField;
	char* ___U3CMemberNameU3Ek__BackingField;
	Type_t* ____attributeType;
	MemberInfo_t* ____lazyMemberInfo;
};
struct CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02_marshaled_com
{
	CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F_marshaled_com ___U3CTypedValueU3Ek__BackingField;
	int32_t ___U3CIsFieldU3Ek__BackingField;
	Il2CppChar* ___U3CMemberNameU3Ek__BackingField;
	Type_t* ____attributeType;
	MemberInfo_t* ____lazyMemberInfo;
};
struct Delegate_t  : public RuntimeObject
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	RuntimeObject* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	bool ___method_is_virtual;
};
struct Delegate_t_marshaled_pinvoke
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct Delegate_t_marshaled_com
{
	intptr_t ___method_ptr;
	intptr_t ___invoke_impl;
	Il2CppIUnknown* ___m_target;
	intptr_t ___method;
	intptr_t ___delegate_trampoline;
	intptr_t ___extra_arg;
	intptr_t ___method_code;
	intptr_t ___interp_method;
	intptr_t ___interp_invoke_impl;
	MethodInfo_t* ___method_info;
	MethodInfo_t* ___original_method_info;
	DelegateData_t9B286B493293CD2D23A5B2B5EF0E5B1324C2B77E* ___data;
	int32_t ___method_is_virtual;
};
struct Exception_t  : public RuntimeObject
{
	String_t* ____className;
	String_t* ____message;
	RuntimeObject* ____data;
	Exception_t* ____innerException;
	String_t* ____helpURL;
	RuntimeObject* ____stackTrace;
	String_t* ____stackTraceString;
	String_t* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	RuntimeObject* ____dynamicMethods;
	int32_t ____HResult;
	String_t* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	IntPtrU5BU5D_tFD177F8C806A6921AD7150264CCC62FA00CAD832* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_pinvoke
{
	char* ____className;
	char* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_pinvoke* ____innerException;
	char* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	char* ____stackTraceString;
	char* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	char* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct Exception_t_marshaled_com
{
	Il2CppChar* ____className;
	Il2CppChar* ____message;
	RuntimeObject* ____data;
	Exception_t_marshaled_com* ____innerException;
	Il2CppChar* ____helpURL;
	Il2CppIUnknown* ____stackTrace;
	Il2CppChar* ____stackTraceString;
	Il2CppChar* ____remoteStackTraceString;
	int32_t ____remoteStackIndex;
	Il2CppIUnknown* ____dynamicMethods;
	int32_t ____HResult;
	Il2CppChar* ____source;
	SafeSerializationManager_tCBB85B95DFD1634237140CD892E82D06ECB3F5E6* ____safeSerializationManager;
	StackTraceU5BU5D_t32FBCB20930EAF5BAE3F450FF75228E5450DA0DF* ___captured_traces;
	Il2CppSafeArray* ___native_trace_ips;
	int32_t ___caught_in_unmanaged;
};
struct GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8  : public RuntimeObject
{
	bool ___m_disposed;
	intptr_t ___m_jobject;
};
struct HandleRef_t4B05E32B68797F702257D4E838B85A976313F08F 
{
	RuntimeObject* ____wrapper;
	intptr_t ____handle;
};
struct Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D 
{
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f0;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f1;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f2;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f3;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f4;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f5;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f6;
	Long8_t47945FFD68CACB5C8CE8E03AECE69B817A6A9659 ___f7;
};
struct RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B 
{
	intptr_t ___value;
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225 
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			bool ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			bool ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			Il2CppChar ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			Il2CppChar ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_marshaled_pinvoke
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225_marshaled_com
{
	union
	{
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___z;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___z_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int8_t ___b;
		};
		#pragma pack(pop, tp)
		struct
		{
			int8_t ___b_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			uint8_t ___c;
		};
		#pragma pack(pop, tp)
		struct
		{
			uint8_t ___c_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int16_t ___s;
		};
		#pragma pack(pop, tp)
		struct
		{
			int16_t ___s_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int32_t ___i;
		};
		#pragma pack(pop, tp)
		struct
		{
			int32_t ___i_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			int64_t ___j;
		};
		#pragma pack(pop, tp)
		struct
		{
			int64_t ___j_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			float ___f;
		};
		#pragma pack(pop, tp)
		struct
		{
			float ___f_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			double ___d;
		};
		#pragma pack(pop, tp)
		struct
		{
			double ___d_forAlignmentOnly;
		};
		#pragma pack(push, tp, 1)
		struct
		{
			intptr_t ___l;
		};
		#pragma pack(pop, tp)
		struct
		{
			intptr_t ___l_forAlignmentOnly;
		};
	};
};
struct Range_tB5BAD1274CA0989FC97B0093B4149EF3CD5F21AC 
{
	intptr_t ___Pointer;
	int32_t ___Items;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Allocator;
};
struct TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE 
{
	intptr_t ___function;
	intptr_t ___state;
};
struct NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 
{
	void* ___m_Buffer;
	int32_t ___m_Length;
	int32_t ___m_AllocatorLabel;
};
struct Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 
{
	ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC ____pointer;
	int32_t ____length;
};
struct FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25  : public RuntimeObject
{
	HandleRef_t4B05E32B68797F702257D4E838B85A976313F08F ___swigCPtr;
	bool ___swigCMemOwn;
	String_t* ___name;
	EventHandler_tC6323FD7E6163F965259C33D72612C0E5B9BAB82* ___AppDisposed;
	FirebaseAppPlatform_t5AD8517EA34467536BAC8C7C6EB4D4B6880312A2* ___appPlatform;
};
struct FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40  : public RuntimeObject
{
	HandleRef_t4B05E32B68797F702257D4E838B85A976313F08F ___swigCPtr;
	bool ___swigCMemOwn;
};
struct Long512_t2D339FF6672EB3709A6C638EABA7D13C7FEC1878 
{
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f0;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f1;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f2;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f3;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f4;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f5;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f6;
	Long64_t9C8641A91D7FEF90D4B55FDC6B5823A59670156D ___f7;
};
struct MulticastDelegate_t  : public Delegate_t
{
	DelegateU5BU5D_tC5AB7E8F745616680F337909D3A8E6C722CDF771* ___delegates;
};
struct MulticastDelegate_t_marshaled_pinvoke : public Delegate_t_marshaled_pinvoke
{
	Delegate_t_marshaled_pinvoke** ___delegates;
};
struct MulticastDelegate_t_marshaled_com : public Delegate_t_marshaled_com
{
	Delegate_t_marshaled_com** ___delegates;
};
struct RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254 
{
	Spinner_t9606E334089E448EA313C31DC5EFC9345A58BCEB ___m_spinner;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___m_handle;
	UnmanagedArray_1_t7A336330780E05C924BB57ED337DDD2A66701FAB ___m_block;
	int32_t ___m_last;
	int32_t ___m_used;
	uint8_t ___m_enableBlockFree;
	uint8_t ___m_reachMaxBlockSize;
};
struct SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295  : public Exception_t
{
};
struct Type_t  : public MemberInfo_t
{
	RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ____impl;
};
struct Block_tCCF620817FE305B5BF7B0FB7705B4571F976C4E3 
{
	Range_tB5BAD1274CA0989FC97B0093B4149EF3CD5F21AC ___Range;
	int32_t ___BytesPerItem;
	int32_t ___AllocatedItems;
	uint8_t ___Log2Alignment;
	uint8_t ___Padding0;
	uint16_t ___Padding1;
	uint32_t ___Padding2;
};
struct Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457  : public MulticastDelegate_t
{
};
struct Func_1_tBB8824FA8746333BFFF3AB3CE4A41B58450AF431  : public MulticastDelegate_t
{
};
struct ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263  : public SystemException_tCC48D868298F4C0705279823E34B00F4FBDB7295
{
	String_t* ____paramName;
};
struct Long1024_tEE887C506947419DC829213E6C7483D80AF5659F 
{
	Long512_t2D339FF6672EB3709A6C638EABA7D13C7FEC1878 ___f0;
	Long512_t2D339FF6672EB3709A6C638EABA7D13C7FEC1878 ___f1;
};
struct TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA  : public MulticastDelegate_t
{
};
struct ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
};
struct ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F  : public ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263
{
	RuntimeObject* ____actualValue;
};
struct AllocatorCache_1_tE4A3FB98A13F6767AD7FC224210A0475AE15E18E_StaticFields
{
	FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C ___TryFunction;
	TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* ___CachedFunction;
};
struct ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28_StaticFields
{
	ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* ___s_defaultArraySortHelper;
};
struct AllocatorManager_tFB15A22029C8159A3DCD4C08935BE57D3E6B3C2C_StaticFields
{
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Invalid;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___None;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Temp;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___TempJob;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___Persistent;
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ___AudioKernel;
	uint16_t ___NumGlobalScratchAllocators;
	uint16_t ___MaxNumGlobalAllocators;
	uint32_t ___GlobalAllocatorBaseIndex;
	uint32_t ___FirstGlobalScratchpadAllocatorIndex;
};
struct AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_StaticFields
{
	bool ___enableDebugPrints;
};
struct AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields
{
	Uri_t1500A52B5F71A04F5D05C0852D0F2A0941842A0E* ___DefaultUpdateUrl;
	String_t* ___Default;
	RuntimeObject* ___Sync;
	AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48* ____instance;
	Dictionary_2_t73D7B210F00FD83B4F194279B052C32E9FCB1D04* ___SStringState;
};
struct FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_StaticFields
{
	FirebaseMonoBehaviour_t0DC02A14DFF90538B38698592F07B4373DB03C72* ___firebaseMonoBehaviour;
	RuntimeObject* ___U3CAppUtilsU3Ek__BackingField;
	int32_t ___tickCount;
	Dispatcher_tBD1370511B5D6C10B211FBD3AF8E4F9D6B3CD8C6* ___U3CThreadDispatcherU3Ek__BackingField;
	FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586* ___firebaseHandler;
};
struct String_t_StaticFields
{
	String_t* ___Empty;
};
struct IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_StaticFields
{
	SharedStatic_1_t93EB5AFD7E0C5BF92AC0053F6F64C16421DCA08C ___Ref;
};
struct IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_StaticFields
{
	SharedStatic_1_t93EB5AFD7E0C5BF92AC0053F6F64C16421DCA08C ___Ref;
};
struct Boolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_StaticFields
{
	String_t* ___TrueString;
	String_t* ___FalseString;
};
struct Char_t521A6F19B456D956AF452D926C32709DC03D6B17_StaticFields
{
	ByteU5BU5D_tA6237BF417AE52AD70CFB4EF24A7A82613DF9031* ___s_categoryForLatin1;
};
struct IntPtr_t_StaticFields
{
	intptr_t ___Zero;
};
struct AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_StaticFields
{
	GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* ___s_JavaLangSystemClass;
	intptr_t ___s_HashCodeMethodID;
};
struct Exception_t_StaticFields
{
	RuntimeObject* ___s_EDILock;
};
struct FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_StaticFields
{
	RuntimeObject* ___disposeLock;
	Dictionary_2_t070EAA8A0D7DC2B4DA1223E3809A83B3933BF21A* ___nameToProxy;
	Dictionary_2_tD81F54C87D78FE70A5DE7DAA170AE5EB4E54E8C3* ___cPtrToProxy;
	bool ___AppUtilCallbacksInitialized;
	RuntimeObject* ___AppUtilCallbacksLock;
	bool ___PreventOnAllAppsDestroyed;
	bool ___crashlyticsInitializationAttempted;
	bool ___userAgentRegistered;
	int32_t ___CheckDependenciesThread;
	RuntimeObject* ___CheckDependenciesThreadLock;
};
struct Type_t_StaticFields
{
	Binder_t91BFCE95A7057FADF4D8A1A342AFE52872246235* ___s_defaultBinder;
	Il2CppChar ___Delimiter;
	TypeU5BU5D_t97234E1129B564EB38B8D85CAC2AD8B5B9522FFB* ___EmptyTypes;
	RuntimeObject* ___Missing;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterAttribute;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterName;
	MemberFilter_tF644F1AE82F611B677CE1964D5A3277DDA21D553* ___FilterNameIgnoreCase;
};
#ifdef __clang__
#pragma clang diagnostic pop
#endif
struct ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918  : public RuntimeArray
{
	ALIGN_FIELD (8) RuntimeObject* m_Items[1];

	inline RuntimeObject* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, RuntimeObject* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline RuntimeObject* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline RuntimeObject** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, RuntimeObject* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC  : public RuntimeArray
{
	ALIGN_FIELD (8) uint8_t m_Items[1];

	inline uint8_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + il2cpp_array_calc_byte_offset(this, index);
	}
	inline uint8_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + il2cpp_array_calc_byte_offset(this, index);
	}
};
struct CustomAttributeNamedArgumentU5BU5D_tC0A39D9401E28662213F5958EFF5D26D0681B440  : public RuntimeArray
{
	ALIGN_FIELD (8) CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02 m_Items[1];

	inline CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((&((m_Items + index)->___U3CTypedValueU3Ek__BackingField))->___U3CArgumentTypeU3Ek__BackingField), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((&((m_Items + index)->___U3CTypedValueU3Ek__BackingField))->___U3CValueU3Ek__BackingField), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___U3CMemberNameU3Ek__BackingField), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->____attributeType), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->____lazyMemberInfo), (void*)NULL);
		#endif
	}
	inline CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, CustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02 value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((&((m_Items + index)->___U3CTypedValueU3Ek__BackingField))->___U3CArgumentTypeU3Ek__BackingField), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((&((m_Items + index)->___U3CTypedValueU3Ek__BackingField))->___U3CValueU3Ek__BackingField), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___U3CMemberNameU3Ek__BackingField), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->____attributeType), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->____lazyMemberInfo), (void*)NULL);
		#endif
	}
};
struct CustomAttributeTypedArgumentU5BU5D_t6CAA09EC6AACBED57FC8B02C587D50BF6B738C6B  : public RuntimeArray
{
	ALIGN_FIELD (8) CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F m_Items[1];

	inline CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___U3CArgumentTypeU3Ek__BackingField), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___U3CValueU3Ek__BackingField), (void*)NULL);
		#endif
	}
	inline CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, CustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___U3CArgumentTypeU3Ek__BackingField), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___U3CValueU3Ek__BackingField), (void*)NULL);
		#endif
	}
};
struct __CanonU5BU5D_tFF96AE6C231BB36A6CEE54CEEB72ED8E90201979  : public RuntimeArray
{
	ALIGN_FIELD (8) Il2CppSharedGenericObject* m_Items[1];

	inline Il2CppSharedGenericObject* GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline Il2CppSharedGenericObject** GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, Il2CppSharedGenericObject* value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
	inline Il2CppSharedGenericObject* GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline Il2CppSharedGenericObject** GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, Il2CppSharedGenericObject* value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)m_Items + index, (void*)value);
	}
};
struct UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299  : public RuntimeArray
{
	ALIGN_FIELD (8) uint64_t m_Items[1];

	inline uint64_t GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline uint64_t* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, uint64_t value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
	}
	inline uint64_t GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline uint64_t* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, uint64_t value)
	{
		m_Items[index] = value;
	}
};
struct SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B  : public RuntimeArray
{
	ALIGN_FIELD (8) SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 m_Items[1];

	inline SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___AssemblyName), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___Assembly), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___Dependencies), (void*)NULL);
		#endif
	}
	inline SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___AssemblyName), (void*)NULL);
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___Assembly), (void*)NULL);
		#endif
		#if IL2CPP_ENABLE_STRICT_WRITE_BARRIERS
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___Dependencies), (void*)NULL);
		#endif
	}
};
struct CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466  : public RuntimeArray
{
	ALIGN_FIELD (8) CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 m_Items[1];

	inline CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 GetAt(il2cpp_array_size_t index) const
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items[index];
	}
	inline CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565* GetAddressAt(il2cpp_array_size_t index)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		return m_Items + index;
	}
	inline void SetAt(il2cpp_array_size_t index, CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 value)
	{
		IL2CPP_ARRAY_BOUNDS_CHECK(index, (uint32_t)(this)->max_length);
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___dstGpuBuffer), (void*)NULL);
	}
	inline CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 GetAtUnchecked(il2cpp_array_size_t index) const
	{
		return m_Items[index];
	}
	inline CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565* GetAddressAtUnchecked(il2cpp_array_size_t index)
	{
		return m_Items + index;
	}
	inline void SetAtUnchecked(il2cpp_array_size_t index, CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 value)
	{
		m_Items[index] = value;
		Il2CppCodeGenWriteBarrier((void**)&((m_Items + index)->___dstGpuBuffer), (void*)NULL);
	}
};


IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void FunctionPointer_1__ctor_mE3820F1453217EF8118D6A53E1B8CA7AFF1E463C_gshared_inline (FunctionPointer_1_t57A2ADDF748E323E76FF7DAD737F07BA9549251E* __this, intptr_t ___0_ptr, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR FunctionPointer_1_t57A2ADDF748E323E76FF7DAD737F07BA9549251E BurstCompiler_CompileFunctionPointer_TisIl2CppSharedGenericObject_m8B8A4978D08D5F76B623BA56703B29F831E980BA_gshared (Il2CppSharedGenericObject* ___0_delegateMethod, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_gshared (SharedStatic_1_t93EB5AFD7E0C5BF92AC0053F6F64C16421DCA08C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_gshared (Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* ___0_t, int32_t ___1_offset, int32_t ___2_bits, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool Func_1_Invoke_mBB7F37C468451AF57FAF31635C544D6B8C4373B2_gshared_inline (Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m181D7F12EB826B7D6B73742BFD85A667D533BABA_gshared (void* ___0_dataPointer, int32_t ___1_length, int32_t ___2_allocator, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mD9D29EE5CE27F4BFD0168818BF064EC5B3672F39_gshared (void* ___0_dataPointer, int32_t ___1_length, int32_t ___2_allocator, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool NativeArray_1_get_IsCreated_mD74FCA194584E6EA7916853B62401EB78240A081_gshared_inline (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void* NativeArrayUnsafeUtility_GetUnsafePtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m8CFDB2DF56E810A2E2FB3686AF676FCAC65AFCC2_gshared_inline (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF ___0_nativeArray, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool NativeArray_1_get_IsCreated_mF1154ABCB79F43B3A71BFA8DCB85ACBDFA2838D5_gshared_inline (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190* __this, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void* NativeArrayUnsafeUtility_GetUnsafePtr_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mCD832837FF5CE235B506272835BA049D56968E2F_gshared_inline (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 ___0_nativeArray, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool _AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mBF1AB84DE0B50E7C206BAD3C170D1F5AD2D9E343_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar _AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m9ECB51B699AB7B96368F39CC9F7C481340DD0BBC_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double _AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m1F7F249CC7CDA2D6E136C627D983569B3674EA73_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t _AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m901F767E3189385BB47B5B021C40507AAF67D477_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t _AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m37A94A4CA9F6456009CABE27D4AA2ED7B8365168_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t _AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mBE697F1C4D60A7F2BB06784B5CB0C540B7FBF664_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t _AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m01777984AFB915C1167148BC6056336EB200B151_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float _AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6634501B2BC58937A4D852F6F2D8D55A9DFB6F64_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* _AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_mAD2432704589B67F3F93D55DF8B948DCBEBA4444_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFD988C7487D7A7810D33F985E48AB82A006E1B7B_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m646B71E5782A91A5A247FFAEEA04D44FB31AA619_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mC4E70171E726BFD2EDF128963FAD90873B0289C5_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m5C5661781E8C49E962C2556FC75101FD0E862AFD_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m465627642B00BC71088A5883058BB86DDBADABE0_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mA32D3320555971B585058A28FC5F5EC84DC10A34_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF0D2DB5B885B01587820A9F88C6CAD67C8DC1512_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB340108BAAC2DA25B45971B719093FDF9B13E745_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mF114D8205E13517DE4F8DB9772E9C40C7C43BD85_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m362BBB1CC3B8834A056A8F709649BD182E24E9BD_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m86B190CF71EDCB08177B90F98C76A9545AF09FCB_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t _AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_mFEAB91A44B644CE56BB6182931D1065B679E0B3A_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m72E1F5D2CE435E45651E8B970A73B9F5E3E03815_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m22710813CDD982DC5F88C2A4067FBFE1D57A3065_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m946CEC22A94586DE93BE0725E5F25E04589F69DE_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m08A0CE3C4BDDA756095A0A816C659587814F649E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1BFF486C91846170C07AAF27EF84971FF0596B90_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mC6DBECDD44973120E0A56068ABE886D9A7016F5C_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mAE2F0D59E0CE54D36BB86C53218704FA72DC0350_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m71549B5031FEE0FBB78A9CE02EDADD14DBFFB56D_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m39A1E2B88CD5364F83417EFB65F1C296656D2F62_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m41529A6A47CC3B57286AC8F9C6388E54918DF450_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEE64E87A5A2BC0F38910643A9B91032AF38D882C_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mB41AF2746C52AC8CB15EFFA72BF98A34742B7690_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_m8E1513A9554AAA1822CA51D0A816DCF16775AA37_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m0207DDD0AB7EE1136083B85289D4F16FD9A64ECC_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m51DC292506277213F541434A51F81430E11E7226_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mF6AF89E7365D44A201C5A2C86C49EF49E2D6ADAD_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF6FCB78A4CF7C2FB0375E88C31C70458F55F422C_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m02707C23958FF03FA0345C2A17F66EE379A1468C_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m0CADE835010213386C1C07E63F70E55498142C20_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m2BD2E924FBD117B44A2D5E741144FD7B1FCE918E_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m93BCF3156F5C823F8C2B005523A618966B0A6DBB_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_m6F1CD10E4BE61A7211FB87DA1D7D92C7FE85F8E4_gshared (intptr_t ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDED207A2CABEE9D78BEC022F1D4338A5091DCC50_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m599A48B8B43BD171290DD40A653B639B6F856D42_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4289B7D740963C5007AD93A07FF6A0A01857DD28_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_m177F1A057E8F5EF05D9158BA272E890C6210044B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject_FromJavaArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mA1CB7EC9473B0674763C89AC566EC664159108E9_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mE3914411BE312CBBA869835158E535A06CD3EEB4_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m73D49A76D8F24AD1815CF94E87F1EF4683A38EA6_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJavaObject_FromJavaArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m18459EC016E866EF307EC4D9442CD86FD9BC28B4_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m791647AA6EAD3B88467418F51CF53F5755AB38BA_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m8B3C7C06394410B77DB9953832CB9F8C29C28E89_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJavaObject_FromJavaArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m3CAD187DEBCBD88D94604015E86A37B8944847BC_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m44C60AAE64EE97BFD5D9D302E7BE5BE20E6862B3_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m0BA90472306DF5C3A1491FC3EB052DD58FA468C3_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJavaObject_FromJavaArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m62441E1DA5798B8F585F5EF18386C4FB739BFFC8_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m3B57915B048BD436E00741CD146D62AC775E7BA3_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mA616697346ABF4A696B8F63B5F85AA92777EA400_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_FromJavaArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mC1390BDE8BF42FAA78215DFE076A25F7AEA674D7_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mF87CC238DDBB3C90C35846E626548847CC7E319D_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m0D60BAB951F2C313123D62CAB81EEA263F2F0750_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJavaObject_FromJavaArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m6A8E0C87ADD6FC1CA3F936C0CC0AC4AB379A8422_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m9665934CC08FD80EB16882C4EA6FDBC5D9C3A175_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4B925872A7B673BC9981AB72E3A229BBE0E1119E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJavaObject_FromJavaArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mB474FE99696A6DEB269E0F1C3C7DA32CD8854410_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mEF6ACD1C6C5237E0446DB9DBDAC40BA2660849BB_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mA3300F1D8CC0B110426EAE23F5FB7F27FD8AE76B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject_FromJavaArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mD5A794B10305822230E288BE2D72329A6EE0C8A4_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mA3FFDC6948922BEFC787A297D5483BA277EEFF0C_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8EDEAF89FE2C5B65277B0FC57FAD4FA9253E1140_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject_FromJavaArray_TisIl2CppSharedGenericObject_m22D9898503E929D77563392822CD11A056613AFF_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_m95122CB55DFAF463B14CBBA51C2B0811790DB90B_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m34A45129A0AA339AC2E31492EF928112EDECC197_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m9EDC162A865064239ADA4058CD2A81B1A2CF6D0F_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4F31B91D2F03C3068A4BE95DB368B6DF715CE316_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m3F27BD182204628FAEA59AC70923AEB50F5B8B44_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_mF9D318147E29742DAA4B5C033BFC68D29CC1123B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m61DD76EF7E812BC3ED41BBA0D0DA1B430EC61256_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1B394DCE5ED40DFA09EB18874BA2FFC3B5E50B2E_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB506BB319D4A193AE597F3C3166DDB165593D4D6_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6191F97DB89FB7E398F8A3DF9E448A9DCD5F07EA_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mDD609864F766F77FB093EC507721536574D38177_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m0EB49046BB127E247EF96EA1C8BB5BC0D4F4B439_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m86131C9F9AC1A6082963096E96DDDDDD7366189E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisIl2CppSharedGenericObject_mB1CEFE624E92B82D234C1982AA2A7DF6AC5FD284_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mDFF491584A820BE0629A47ED3B790A199050D589_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_mF12B3A136CBDB98DF0876DF1FC47EA4A14D98A17_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Dictionary_2_TryGetValue_mE84CAE1C2C3C804992294A454700ECB3273CA55C_gshared (Dictionary_2_t36B17C1CDA9BE4EE8C94D25A0DC9A7F4C75D51D7* __this, int32_t ___0_key, Il2CppSharedGenericObject** ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Dictionary_2__ctor_mE9A1CF09D5006AC41488691817533288065B42FF_gshared (Dictionary_2_tB3BF685342B8A1AB5995EE46B7014CBC0B281D5C* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Dictionary_2_set_Item_m2BB6CF3A683DE145DB22935870F3C63F3E8E3027_gshared (Dictionary_2_t36B17C1CDA9BE4EE8C94D25A0DC9A7F4C75D51D7* __this, int32_t ___0_key, Il2CppSharedGenericObject* ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Dictionary_2_TryGetValue_mEB1F53213D9DBB5055E1D48D152CB9B8A5A10437_gshared (Dictionary_2_tB3BF685342B8A1AB5995EE46B7014CBC0B281D5C* __this, Il2CppSharedGenericObject* ___0_key, Il2CppSharedGenericObject** ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Dictionary_2_set_Item_m970F39957AA13F57797915183F66166CF35746AD_gshared (Dictionary_2_tB3BF685342B8A1AB5995EE46B7014CBC0B281D5C* __this, Il2CppSharedGenericObject* ___0_key, Il2CppSharedGenericObject* ___1_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ReadOnlyCollection_1__ctor_m010AA1E4CD3820BCC07B77A482D6A17D5C7F14C2_gshared (ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050* __this, RuntimeObject* ___0_list, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ReadOnlyCollection_1__ctor_mE22404DEC336AFE9817BF78FF9F7F90E07EE4F7A_gshared (ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513* __this, RuntimeObject* ___0_list, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ReadOnlyCollection_1__ctor_mC1890FAC00703F47A655C35CBCB613C74A811580_gshared (ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263* __this, RuntimeObject* ___0_list, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m0AA0BC859F6008F37B680B00508C065B098C7D9A_gshared (UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, int32_t ___1_index, int32_t ___2_length, uint64_t ___3_value, RuntimeObject* ___4_comparer, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisSortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53_m8F7822162DB61B62B083C2CD23E5F632B85BF601_gshared (SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B* ___0_array, int32_t ___1_index, int32_t ___2_length, SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 ___3_value, RuntimeObject* ___4_comparer, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* ArraySortHelper_1_get_Default_m9357CBDC001EFB2BA8D065940A5C9FEFED3CC036_gshared_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t ArraySortHelper_1_BinarySearch_m65877F6FF5C5E15405D3326440C99808F156B993_gshared (ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* __this, CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466* ___0_array, int32_t ___1_index, int32_t ___2_length, CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 ___3_value, RuntimeObject* ___4_comparer, const RuntimeMethod* method) ;

inline void FunctionPointer_1__ctor_mA6464FB1EEC3C76906932127ADC88D71257A9CB6_inline (FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C* __this, intptr_t ___0_ptr, const RuntimeMethod* method)
{
	((  void (*) (FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C*, intptr_t, const RuntimeMethod*))FunctionPointer_1__ctor_mE3820F1453217EF8118D6A53E1B8CA7AFF1E463C_gshared_inline)(__this, ___0_ptr, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool Delegate_op_Inequality_mA9EAADBA0C976289CCD49DC5A4BEDBB060B579E0 (Delegate_t* ___0_d1, Delegate_t* ___1_d2, const RuntimeMethod* method) ;
inline FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C BurstCompiler_CompileFunctionPointer_TisTryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA_m24AC0F77E9E5A8C61B911FBAF3DC753E0B1FB4DC (TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* ___0_delegateMethod, const RuntimeMethod* method)
{
	return ((  FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C (*) (TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA*, const RuntimeMethod*))BurstCompiler_CompileFunctionPointer_TisIl2CppSharedGenericObject_m8B8A4978D08D5F76B623BA56703B29F831E980BA_gshared)(___0_delegateMethod, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D (void* ___0_value, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 AllocatorManager_Register_mCA48F5A5C481281C97CA4E0D1E934CB771D00237 (intptr_t ___0_allocatorState, FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C ___1_functionPointer, bool ___2_IsAutoDispose, bool ___3_isGlobal, int32_t ___4_globalIndex, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Managed_RegisterDelegate_m494ED81F0C1945174DD1E82D6711EB329255E36F (int32_t ___0_index, TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* ___1_function, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AllocatorHandle_get_IsInstalled_mB38CD887177A87128DC9A2DE6F866F9EC18FA907 (AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AllocatorHandle_Install_m0526A06766A02754698DE0115B926C15566CDD3B (AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* __this, TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE ___0_tableEntry, const RuntimeMethod* method) ;
inline Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545 (SharedStatic_1_t93EB5AFD7E0C5BF92AC0053F6F64C16421DCA08C* __this, const RuntimeMethod* method)
{
	return ((  Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* (*) (SharedStatic_1_t93EB5AFD7E0C5BF92AC0053F6F64C16421DCA08C*, const RuntimeMethod*))SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_gshared)(__this, method);
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline (AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* __this, const RuntimeMethod* method) ;
inline int32_t ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323 (Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* ___0_t, int32_t ___1_offset, int32_t ___2_bits, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Long1024_tEE887C506947419DC829213E6C7483D80AF5659F*, int32_t, int32_t, const RuntimeMethod*))ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_gshared)(___0_t, ___1_offset, ___2_bits, method);
}
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 RewindableAllocator_get_Handle_mF81EDA2102485C46965AAB56347E8F64FE551D9E_inline (RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AllocatorHandle_Dispose_mB74CBC8980962C016A6C85F09D3F94775A2C58E3 (AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Managed_UnregisterDelegate_mE60C615D7705C878A66FB0AA06C7EE86A2359D46 (int32_t ___0_index, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 AllocatorHandle_get_Handle_m440EA9B9A4306115087775DA2AA0AC034107D0E2 (AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA (RuntimeObject* ___0_obj, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149 (RuntimeObject* ___0_obj, bool* ___1_lockTaken, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool FirebaseCrashlyticsInternal_get_IsDisposed_m47E32597ACDF3FAC963438574982F9D43965EEB2 (FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40* __this, const RuntimeMethod* method) ;
inline bool Func_1_Invoke_mBB7F37C468451AF57FAF31635C544D6B8C4373B2_inline (Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457*, const RuntimeMethod*))Func_1_Invoke_mBB7F37C468451AF57FAF31635C544D6B8C4373B2_gshared_inline)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidImpl_LogOperationFailedWarningDueToShutdown_m7FF1B7FA100D2AA4AE5D871CB6845FA5B0FE0FCF (AndroidImpl_t09BB72854905028A1DF3FBA8772392723D2CCD76* __this, String_t* ___0_operation, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline (intptr_t ___0_value1, intptr_t ___1_value2, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t* AndroidJNI_GetDirectBufferAddress_m0E0B127BFEB7AAF065829DC6AE11163D5616EBE8 (intptr_t ___0_buffer, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJNI_GetDirectBufferCapacity_mCAC8D9C8E45481BE59FB17406E1E16D4F9628183 (intptr_t ___0_buffer, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987 (String_t* ___0_format, RuntimeObject* ___1_arg0, RuntimeObject* ___2_arg1, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F (Exception_t* __this, String_t* ___0_message, const RuntimeMethod* method) ;
inline NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m181D7F12EB826B7D6B73742BFD85A667D533BABA (void* ___0_dataPointer, int32_t ___1_length, int32_t ___2_allocator, const RuntimeMethod* method)
{
	return ((  NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF (*) (void*, int32_t, int32_t, const RuntimeMethod*))NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m181D7F12EB826B7D6B73742BFD85A667D533BABA_gshared)(___0_dataPointer, ___1_length, ___2_allocator, method);
}
inline NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mD9D29EE5CE27F4BFD0168818BF064EC5B3672F39 (void* ___0_dataPointer, int32_t ___1_length, int32_t ___2_allocator, const RuntimeMethod* method)
{
	return ((  NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 (*) (void*, int32_t, int32_t, const RuntimeMethod*))NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mD9D29EE5CE27F4BFD0168818BF064EC5B3672F39_gshared)(___0_dataPointer, ___1_length, ___2_allocator, method);
}
inline bool NativeArray_1_get_IsCreated_mD74FCA194584E6EA7916853B62401EB78240A081_inline (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF*, const RuntimeMethod*))NativeArray_1_get_IsCreated_mD74FCA194584E6EA7916853B62401EB78240A081_gshared_inline)(__this, method);
}
inline void* NativeArrayUnsafeUtility_GetUnsafePtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m8CFDB2DF56E810A2E2FB3686AF676FCAC65AFCC2_inline (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF ___0_nativeArray, const RuntimeMethod* method)
{
	return ((  void* (*) (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF, const RuntimeMethod*))NativeArrayUnsafeUtility_GetUnsafePtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m8CFDB2DF56E810A2E2FB3686AF676FCAC65AFCC2_gshared_inline)(___0_nativeArray, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNI_NewDirectByteBuffer_m17389ED6D98CC0364180BAB43F2747B48FFDB107 (uint8_t* ___0_buffer, int64_t ___1_capacity, const RuntimeMethod* method) ;
inline bool NativeArray_1_get_IsCreated_mF1154ABCB79F43B3A71BFA8DCB85ACBDFA2838D5_inline (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190* __this, const RuntimeMethod* method)
{
	return ((  bool (*) (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190*, const RuntimeMethod*))NativeArray_1_get_IsCreated_mF1154ABCB79F43B3A71BFA8DCB85ACBDFA2838D5_gshared_inline)(__this, method);
}
inline void* NativeArrayUnsafeUtility_GetUnsafePtr_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mCD832837FF5CE235B506272835BA049D56968E2F_inline (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 ___0_nativeArray, const RuntimeMethod* method)
{
	return ((  void* (*) (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190, const RuntimeMethod*))NativeArrayUnsafeUtility_GetUnsafePtr_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mCD832837FF5CE235B506272835BA049D56968E2F_gshared_inline)(___0_nativeArray, method);
}
inline bool _AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mBF1AB84DE0B50E7C206BAD3C170D1F5AD2D9E343 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  bool (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mBF1AB84DE0B50E7C206BAD3C170D1F5AD2D9E343_gshared)(___0_array, method);
}
inline Il2CppChar _AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m9ECB51B699AB7B96368F39CC9F7C481340DD0BBC (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  Il2CppChar (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m9ECB51B699AB7B96368F39CC9F7C481340DD0BBC_gshared)(___0_array, method);
}
inline double _AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m1F7F249CC7CDA2D6E136C627D983569B3674EA73 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  double (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m1F7F249CC7CDA2D6E136C627D983569B3674EA73_gshared)(___0_array, method);
}
inline int16_t _AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m901F767E3189385BB47B5B021C40507AAF67D477 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int16_t (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m901F767E3189385BB47B5B021C40507AAF67D477_gshared)(___0_array, method);
}
inline int32_t _AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m37A94A4CA9F6456009CABE27D4AA2ED7B8365168 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int32_t (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m37A94A4CA9F6456009CABE27D4AA2ED7B8365168_gshared)(___0_array, method);
}
inline int64_t _AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mBE697F1C4D60A7F2BB06784B5CB0C540B7FBF664 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int64_t (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mBE697F1C4D60A7F2BB06784B5CB0C540B7FBF664_gshared)(___0_array, method);
}
inline int8_t _AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m01777984AFB915C1167148BC6056336EB200B151 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int8_t (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m01777984AFB915C1167148BC6056336EB200B151_gshared)(___0_array, method);
}
inline float _AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6634501B2BC58937A4D852F6F2D8D55A9DFB6F64 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  float (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6634501B2BC58937A4D852F6F2D8D55A9DFB6F64_gshared)(___0_array, method);
}
inline Il2CppSharedGenericObject* _AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_mAD2432704589B67F3F93D55DF8B948DCBEBA4444 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (intptr_t, const RuntimeMethod*))_AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_mAD2432704589B67F3F93D55DF8B948DCBEBA4444_gshared)(___0_array, method);
}
inline intptr_t _AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFD988C7487D7A7810D33F985E48AB82A006E1B7B (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFD988C7487D7A7810D33F985E48AB82A006E1B7B_gshared)(___0_jclass, ___1_fieldName, ___2_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m646B71E5782A91A5A247FFAEEA04D44FB31AA619 (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m646B71E5782A91A5A247FFAEEA04D44FB31AA619_gshared)(___0_jclass, ___1_fieldName, ___2_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mC4E70171E726BFD2EDF128963FAD90873B0289C5 (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mC4E70171E726BFD2EDF128963FAD90873B0289C5_gshared)(___0_jclass, ___1_fieldName, ___2_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m5C5661781E8C49E962C2556FC75101FD0E862AFD (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m5C5661781E8C49E962C2556FC75101FD0E862AFD_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m465627642B00BC71088A5883058BB86DDBADABE0 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m465627642B00BC71088A5883058BB86DDBADABE0_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mA32D3320555971B585058A28FC5F5EC84DC10A34 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mA32D3320555971B585058A28FC5F5EC84DC10A34_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF0D2DB5B885B01587820A9F88C6CAD67C8DC1512 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF0D2DB5B885B01587820A9F88C6CAD67C8DC1512_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB340108BAAC2DA25B45971B719093FDF9B13E745 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB340108BAAC2DA25B45971B719093FDF9B13E745_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mF114D8205E13517DE4F8DB9772E9C40C7C43BD85 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mF114D8205E13517DE4F8DB9772E9C40C7C43BD85_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m362BBB1CC3B8834A056A8F709649BD182E24E9BD (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m362BBB1CC3B8834A056A8F709649BD182E24E9BD_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m86B190CF71EDCB08177B90F98C76A9545AF09FCB (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m86B190CF71EDCB08177B90F98C76A9545AF09FCB_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline intptr_t _AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_mFEAB91A44B644CE56BB6182931D1065B679E0B3A (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))_AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_mFEAB91A44B644CE56BB6182931D1065B679E0B3A_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline bool AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m72E1F5D2CE435E45651E8B970A73B9F5E3E03815 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  bool (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m72E1F5D2CE435E45651E8B970A73B9F5E3E03815_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline Il2CppChar AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m22710813CDD982DC5F88C2A4067FBFE1D57A3065 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  Il2CppChar (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m22710813CDD982DC5F88C2A4067FBFE1D57A3065_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline double AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m946CEC22A94586DE93BE0725E5F25E04589F69DE (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  double (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m946CEC22A94586DE93BE0725E5F25E04589F69DE_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline int16_t AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m08A0CE3C4BDDA756095A0A816C659587814F649E (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int16_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m08A0CE3C4BDDA756095A0A816C659587814F649E_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline int32_t AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1BFF486C91846170C07AAF27EF84971FF0596B90 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1BFF486C91846170C07AAF27EF84971FF0596B90_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline int64_t AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mC6DBECDD44973120E0A56068ABE886D9A7016F5C (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int64_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mC6DBECDD44973120E0A56068ABE886D9A7016F5C_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline int8_t AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mAE2F0D59E0CE54D36BB86C53218704FA72DC0350 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int8_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mAE2F0D59E0CE54D36BB86C53218704FA72DC0350_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline float AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m71549B5031FEE0FBB78A9CE02EDADD14DBFFB56D (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  float (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m71549B5031FEE0FBB78A9CE02EDADD14DBFFB56D_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline Il2CppSharedGenericObject* AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m39A1E2B88CD5364F83417EFB65F1C296656D2F62 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m39A1E2B88CD5364F83417EFB65F1C296656D2F62_gshared)(__this, ___0_methodName, ___1_args, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Call_m2126160FB635069207535BD0E700C3605FDB3308 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Call_m4C4D7D7287030773175BDF47681EA018DFA4DF1A (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
inline bool AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m41529A6A47CC3B57286AC8F9C6388E54918DF450 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  bool (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m41529A6A47CC3B57286AC8F9C6388E54918DF450_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline int32_t AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEE64E87A5A2BC0F38910643A9B91032AF38D882C (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEE64E87A5A2BC0F38910643A9B91032AF38D882C_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline float AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mB41AF2746C52AC8CB15EFFA72BF98A34742B7690 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  float (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mB41AF2746C52AC8CB15EFFA72BF98A34742B7690_gshared)(__this, ___0_methodName, ___1_args, method);
}
inline Il2CppSharedGenericObject* AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_m8E1513A9554AAA1822CA51D0A816DCF16775AA37 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_m8E1513A9554AAA1822CA51D0A816DCF16775AA37_gshared)(__this, ___0_methodName, ___1_args, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__CallStatic_mE917E474DB9801610FB7ABE5BE749DF84CEFD48A (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__CallStatic_mD63902D30CD5626DAEAD1D6484AF7A9ACA85590E (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) ;
inline bool AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m0207DDD0AB7EE1136083B85289D4F16FD9A64ECC (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  bool (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m0207DDD0AB7EE1136083B85289D4F16FD9A64ECC_gshared)(___0_array, method);
}
inline Il2CppChar AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m51DC292506277213F541434A51F81430E11E7226 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  Il2CppChar (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m51DC292506277213F541434A51F81430E11E7226_gshared)(___0_array, method);
}
inline double AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mF6AF89E7365D44A201C5A2C86C49EF49E2D6ADAD (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  double (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mF6AF89E7365D44A201C5A2C86C49EF49E2D6ADAD_gshared)(___0_array, method);
}
inline int16_t AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF6FCB78A4CF7C2FB0375E88C31C70458F55F422C (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int16_t (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF6FCB78A4CF7C2FB0375E88C31C70458F55F422C_gshared)(___0_array, method);
}
inline int32_t AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m02707C23958FF03FA0345C2A17F66EE379A1468C (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int32_t (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m02707C23958FF03FA0345C2A17F66EE379A1468C_gshared)(___0_array, method);
}
inline int64_t AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m0CADE835010213386C1C07E63F70E55498142C20 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int64_t (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m0CADE835010213386C1C07E63F70E55498142C20_gshared)(___0_array, method);
}
inline int8_t AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m2BD2E924FBD117B44A2D5E741144FD7B1FCE918E (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  int8_t (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m2BD2E924FBD117B44A2D5E741144FD7B1FCE918E_gshared)(___0_array, method);
}
inline float AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m93BCF3156F5C823F8C2B005523A618966B0A6DBB (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  float (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m93BCF3156F5C823F8C2B005523A618966B0A6DBB_gshared)(___0_array, method);
}
inline Il2CppSharedGenericObject* AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_m6F1CD10E4BE61A7211FB87DA1D7D92C7FE85F8E4 (intptr_t ___0_array, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (intptr_t, const RuntimeMethod*))AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_m6F1CD10E4BE61A7211FB87DA1D7D92C7FE85F8E4_gshared)(___0_array, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_DeleteLocalRef_m20303564C88A1B90E3D8D7A7D893392E18967094 (intptr_t ___0_localref, const RuntimeMethod* method) ;
inline int32_t AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDED207A2CABEE9D78BEC022F1D4338A5091DCC50 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, const RuntimeMethod*))AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDED207A2CABEE9D78BEC022F1D4338A5091DCC50_gshared)(__this, ___0_fieldName, method);
}
inline float AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m599A48B8B43BD171290DD40A653B639B6F856D42 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method)
{
	return ((  float (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, const RuntimeMethod*))AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m599A48B8B43BD171290DD40A653B639B6F856D42_gshared)(__this, ___0_fieldName, method);
}
inline int32_t AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4289B7D740963C5007AD93A07FF6A0A01857DD28 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, const RuntimeMethod*))AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4289B7D740963C5007AD93A07FF6A0A01857DD28_gshared)(__this, ___0_fieldName, method);
}
inline Il2CppSharedGenericObject* AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_m177F1A057E8F5EF05D9158BA272E890C6210044B (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, String_t*, const RuntimeMethod*))AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_m177F1A057E8F5EF05D9158BA272E890C6210044B_gshared)(__this, ___0_fieldName, method);
}
inline void Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method)
{
	((  void (*) (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7*, void*, int32_t, const RuntimeMethod*))Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_gshared_inline)(__this, ___0_pointer, ___1_length, method);
}
inline int32_t Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method)
{
	return ((  int32_t (*) (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7*, const RuntimeMethod*))Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_gshared_inline)(__this, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10 (int32_t ___0_capacity, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_args, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___1_jniArgs, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609 (intptr_t ___0_ptr, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Type_t* Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57 (RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B ___0_handle, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25 (Type_t* ___0_t, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR intptr_t GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline (GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* ___0_obj, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972 (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9 (RuntimeObject* ___0_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7 (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97 (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6 (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF (intptr_t ___0_obj, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0 (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* __this, intptr_t ___0_jclass, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6 (Type_t* ___0_t, Type_t* ___1_from, const RuntimeMethod* method) ;
inline bool AndroidJavaObject_FromJavaArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mA1CB7EC9473B0674763C89AC566EC664159108E9 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  bool (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mA1CB7EC9473B0674763C89AC566EC664159108E9_gshared)(___0_jobject, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B (String_t* ___0_str0, String_t* ___1_str1, String_t* ___2_str2, const RuntimeMethod* method) ;
inline intptr_t AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mE3914411BE312CBBA869835158E535A06CD3EEB4 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mE3914411BE312CBBA869835158E535A06CD3EEB4_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline bool AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m73D49A76D8F24AD1815CF94E87F1EF4683A38EA6 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  bool (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m73D49A76D8F24AD1815CF94E87F1EF4683A38EA6_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline Il2CppChar AndroidJavaObject_FromJavaArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m18459EC016E866EF307EC4D9442CD86FD9BC28B4 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  Il2CppChar (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m18459EC016E866EF307EC4D9442CD86FD9BC28B4_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m791647AA6EAD3B88467418F51CF53F5755AB38BA (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m791647AA6EAD3B88467418F51CF53F5755AB38BA_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline Il2CppChar AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m8B3C7C06394410B77DB9953832CB9F8C29C28E89 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  Il2CppChar (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m8B3C7C06394410B77DB9953832CB9F8C29C28E89_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline double AndroidJavaObject_FromJavaArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m3CAD187DEBCBD88D94604015E86A37B8944847BC (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  double (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m3CAD187DEBCBD88D94604015E86A37B8944847BC_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m44C60AAE64EE97BFD5D9D302E7BE5BE20E6862B3 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m44C60AAE64EE97BFD5D9D302E7BE5BE20E6862B3_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline double AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m0BA90472306DF5C3A1491FC3EB052DD58FA468C3 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  double (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m0BA90472306DF5C3A1491FC3EB052DD58FA468C3_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline int16_t AndroidJavaObject_FromJavaArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m62441E1DA5798B8F585F5EF18386C4FB739BFFC8 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  int16_t (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m62441E1DA5798B8F585F5EF18386C4FB739BFFC8_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m3B57915B048BD436E00741CD146D62AC775E7BA3 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m3B57915B048BD436E00741CD146D62AC775E7BA3_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline int16_t AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mA616697346ABF4A696B8F63B5F85AA92777EA400 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int16_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mA616697346ABF4A696B8F63B5F85AA92777EA400_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline int32_t AndroidJavaObject_FromJavaArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mC1390BDE8BF42FAA78215DFE076A25F7AEA674D7 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  int32_t (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mC1390BDE8BF42FAA78215DFE076A25F7AEA674D7_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mF87CC238DDBB3C90C35846E626548847CC7E319D (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mF87CC238DDBB3C90C35846E626548847CC7E319D_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline int32_t AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m0D60BAB951F2C313123D62CAB81EEA263F2F0750 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m0D60BAB951F2C313123D62CAB81EEA263F2F0750_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline int64_t AndroidJavaObject_FromJavaArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m6A8E0C87ADD6FC1CA3F936C0CC0AC4AB379A8422 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  int64_t (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m6A8E0C87ADD6FC1CA3F936C0CC0AC4AB379A8422_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m9665934CC08FD80EB16882C4EA6FDBC5D9C3A175 (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m9665934CC08FD80EB16882C4EA6FDBC5D9C3A175_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline int64_t AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4B925872A7B673BC9981AB72E3A229BBE0E1119E (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int64_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4B925872A7B673BC9981AB72E3A229BBE0E1119E_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline int8_t AndroidJavaObject_FromJavaArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mB474FE99696A6DEB269E0F1C3C7DA32CD8854410 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  int8_t (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mB474FE99696A6DEB269E0F1C3C7DA32CD8854410_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mEF6ACD1C6C5237E0446DB9DBDAC40BA2660849BB (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mEF6ACD1C6C5237E0446DB9DBDAC40BA2660849BB_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline int8_t AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mA3300F1D8CC0B110426EAE23F5FB7F27FD8AE76B (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int8_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mA3300F1D8CC0B110426EAE23F5FB7F27FD8AE76B_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline float AndroidJavaObject_FromJavaArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mD5A794B10305822230E288BE2D72329A6EE0C8A4 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  float (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mD5A794B10305822230E288BE2D72329A6EE0C8A4_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mA3FFDC6948922BEFC787A297D5483BA277EEFF0C (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mA3FFDC6948922BEFC787A297D5483BA277EEFF0C_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline float AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8EDEAF89FE2C5B65277B0FC57FAD4FA9253E1140 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  float (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8EDEAF89FE2C5B65277B0FC57FAD4FA9253E1140_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline Il2CppSharedGenericObject* AndroidJavaObject_FromJavaArray_TisIl2CppSharedGenericObject_m22D9898503E929D77563392822CD11A056613AFF (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArray_TisIl2CppSharedGenericObject_m22D9898503E929D77563392822CD11A056613AFF_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_m95122CB55DFAF463B14CBBA51C2B0811790DB90B (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_m95122CB55DFAF463B14CBBA51C2B0811790DB90B_gshared)(___0_jclass, ___1_methodName, ___2_args, ___3_isStatic, method);
}
inline Il2CppSharedGenericObject* AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m34A45129A0AA339AC2E31492EF928112EDECC197 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m34A45129A0AA339AC2E31492EF928112EDECC197_gshared)(__this, ___0_methodID, ___1_args, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJNISafe_CallStaticIntMethod_m915549FA8FD7FB93B57A9708AD759488EA64418C (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJNISafe_CallStaticBooleanMethod_m652685AC18F590965249C0F9B107C00C142595BB (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJNISafe_CallStaticShortMethod_m8330383670ECCD7E24CDD68C419745E486FA6426 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJNISafe_CallStaticLongMethod_mDDE01239BEFCF007ECE05E51A249B3EB5BB61234 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJNISafe_CallStaticFloatMethod_m3F5419A10B9DF599352938B2BAD8866F8F112364 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJNISafe_CallStaticDoubleMethod_m73F1D51601D6849EE480389B4E43AED68C42B2B5 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJNISafe_CallStaticCharMethod_mC4B40190CE095728E823AB8B724ECDC8F4B36155 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* AndroidJNISafe_CallStaticStringMethod_m4E150E34CC6DBF27A955F8DAEE5941D6E10879C0 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387 (intptr_t ___0_clazz, intptr_t ___1_methodID, Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 ___2_args, const RuntimeMethod* method) ;
inline bool AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m9EDC162A865064239ADA4058CD2A81B1A2CF6D0F (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  bool (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m9EDC162A865064239ADA4058CD2A81B1A2CF6D0F_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline int32_t AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4F31B91D2F03C3068A4BE95DB368B6DF715CE316 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4F31B91D2F03C3068A4BE95DB368B6DF715CE316_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline float AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m3F27BD182204628FAEA59AC70923AEB50F5B8B44 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  float (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m3F27BD182204628FAEA59AC70923AEB50F5B8B44_gshared)(__this, ___0_methodID, ___1_args, method);
}
inline Il2CppSharedGenericObject* AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_mF9D318147E29742DAA4B5C033BFC68D29CC1123B (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_mF9D318147E29742DAA4B5C033BFC68D29CC1123B_gshared)(__this, ___0_methodID, ___1_args, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJNISafe_GetIntField_mBD983688B73063DE5C55D320F60F266443FAC97C (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNISafe_GetObjectField_mCF3BB1C38718D6F55081126BC7F6C286B382B275 (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
inline int32_t AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m61DD76EF7E812BC3ED41BBA0D0DA1B430EC61256 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  int32_t (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m61DD76EF7E812BC3ED41BBA0D0DA1B430EC61256_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1B394DCE5ED40DFA09EB18874BA2FFC3B5E50B2E (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1B394DCE5ED40DFA09EB18874BA2FFC3B5E50B2E_gshared)(___0_jclass, ___1_fieldName, ___2_isStatic, method);
}
inline int32_t AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB506BB319D4A193AE597F3C3166DDB165593D4D6 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, const RuntimeMethod*))AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB506BB319D4A193AE597F3C3166DDB165593D4D6_gshared)(__this, ___0_fieldID, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJNISafe_GetFloatField_m1EAA1ED33002BBA28CA2B630521D6BF1B7D3A2E7 (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
inline float AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6191F97DB89FB7E398F8A3DF9E448A9DCD5F07EA (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  float (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6191F97DB89FB7E398F8A3DF9E448A9DCD5F07EA_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mDD609864F766F77FB093EC507721536574D38177 (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mDD609864F766F77FB093EC507721536574D38177_gshared)(___0_jclass, ___1_fieldName, ___2_isStatic, method);
}
inline float AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m0EB49046BB127E247EF96EA1C8BB5BC0D4F4B439 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method)
{
	return ((  float (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, const RuntimeMethod*))AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m0EB49046BB127E247EF96EA1C8BB5BC0D4F4B439_gshared)(__this, ___0_fieldID, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJNISafe_GetBooleanField_m34F37B560A6AEC81B9061FB3B72698C84720435D (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJNISafe_GetSByteField_mAD3B08AA8A97F77CAE17DD25B0F389AFAC2023B1 (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJNISafe_GetShortField_m5D21E87061C1DAC89DF58671C53432D0361F0C6E (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJNISafe_GetLongField_m7DD751358D10BB276D8A95D413B9DFB1E8EE81D8 (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJNISafe_GetDoubleField_mBCBD5E80223EDECC06FA783F34149E3625219074 (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJNISafe_GetCharField_m8301FA96B40E27C032590FE3F8E84A777A4739C3 (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* AndroidJNISafe_GetStringField_mADFCA05D6DE790600B57E90B20F2E75AFC036B0F (intptr_t ___0_obj, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* AndroidJavaObject_AndroidJavaClassDeleteLocalRef_m56C84D7516BCB51A84E8AFDB3FCA46BAF494548F (intptr_t ___0_jclass, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* AndroidJavaObject_AndroidJavaObjectDeleteLocalRef_m2ECEEAF6389ABB9D6B963B8A98568ECD9413DF3C (intptr_t ___0_jobject, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJNISafe_GetStaticIntField_m0698D50C44E490A009E8388C7321630DED5973BD (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867 (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
inline int32_t AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m86131C9F9AC1A6082963096E96DDDDDD7366189E (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method)
{
	return ((  int32_t (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, const RuntimeMethod*))AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m86131C9F9AC1A6082963096E96DDDDDD7366189E_gshared)(__this, ___0_fieldID, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* AndroidJNISafe_GetStaticStringField_mB3D1325B08A38C7DAF1FA3E6CB52F6D8E0A2CB47 (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
inline Il2CppSharedGenericObject* AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisIl2CppSharedGenericObject_mB1CEFE624E92B82D234C1982AA2A7DF6AC5FD284 (intptr_t ___0_jobject, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (intptr_t, const RuntimeMethod*))AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisIl2CppSharedGenericObject_mB1CEFE624E92B82D234C1982AA2A7DF6AC5FD284_gshared)(___0_jobject, method);
}
inline intptr_t AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mDFF491584A820BE0629A47ED3B790A199050D589 (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method)
{
	return ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mDFF491584A820BE0629A47ED3B790A199050D589_gshared)(___0_jclass, ___1_fieldName, ___2_isStatic, method);
}
inline Il2CppSharedGenericObject* AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_mF12B3A136CBDB98DF0876DF1FC47EA4A14D98A17 (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method)
{
	return ((  Il2CppSharedGenericObject* (*) (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*, intptr_t, const RuntimeMethod*))AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_mF12B3A136CBDB98DF0876DF1FC47EA4A14D98A17_gshared)(__this, ___0_fieldID, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJNISafe_GetStaticBooleanField_m172BEAA3F0AB6754EA5F1AD30C36DAA0D3D7C666 (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJNISafe_GetStaticSByteField_m77596E5B1AE58DAFF39268AC954CAD53974A688D (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJNISafe_GetStaticShortField_m83716D4D85B30F26803F866AC47D5C04AAB5D320 (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJNISafe_GetStaticLongField_mABC2B933CEB757E3FAF1FD6C60AA0C4D38E9C49D (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJNISafe_GetStaticFloatField_mD1456B729026959309A839C2647279C0B6541356 (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJNISafe_GetStaticDoubleField_mEB86F2CE1F3879AAA9DEDA4B496F882C0E1DCBC2 (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJNISafe_GetStaticCharField_mF70F6D197261364AF2A9E875D84DDDA35BD0ED96 (intptr_t ___0_clazz, intptr_t ___1_fieldID, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetIntField_mD238DA37BA1B3D7693484237951A6EFEA9C62120 (intptr_t ___0_obj, intptr_t ___1_fieldID, int32_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetBooleanField_m5279EA41B214699E79733DC6C93259CC9DCA1D9E (intptr_t ___0_obj, intptr_t ___1_fieldID, bool ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetSByteField_mB021168746571E7CAA8C0EAD7AA7F02C18B5EE33 (intptr_t ___0_obj, intptr_t ___1_fieldID, int8_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetShortField_mF95E569C142DEDD604CE8BA7617328B3EDDD2F0D (intptr_t ___0_obj, intptr_t ___1_fieldID, int16_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetLongField_m13905547F5CDC7E01AB0D8C787BF98DC2870EC35 (intptr_t ___0_obj, intptr_t ___1_fieldID, int64_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetFloatField_m589CA6B8DD2BFD4515C5AEAE3772782B293F02C3 (intptr_t ___0_obj, intptr_t ___1_fieldID, float ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetDoubleField_mE93D0C5EC2019A1B657BD32970FE6EFC9B005A58 (intptr_t ___0_obj, intptr_t ___1_fieldID, double ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetCharField_m69D09A6A2CEA55D84B240FE32D90300AAB1334F9 (intptr_t ___0_obj, intptr_t ___1_fieldID, Il2CppChar ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStringField_m649363D4E87763D6A9760359EAFB29802E90B409 (intptr_t ___0_obj, intptr_t ___1_fieldID, String_t* ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetObjectField_mFE500926F9C963FF106E8AA30A16F4C671BAA8CA (intptr_t ___0_obj, intptr_t ___1_fieldID, intptr_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJavaProxy_GetRawProxy_m685E066A4D378B596CD88385B954AE90CBF328A9 (AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D* __this, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_ConvertToJNIArray_mBEAE4605FF297D19AFB8CE4E8443C9C0F87E9A13 (RuntimeArray* ___0_array, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticIntField_m1E20F6C72260CAFBF73207DCEC1816B2816EEBE1 (intptr_t ___0_clazz, intptr_t ___1_fieldID, int32_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticBooleanField_mBE4E40DA1B07A29D356AEEE6CB9519F2B3621AC9 (intptr_t ___0_clazz, intptr_t ___1_fieldID, bool ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticSByteField_m242120982A9227E1E8344FFE9F06FD74986D15E9 (intptr_t ___0_clazz, intptr_t ___1_fieldID, int8_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticShortField_m92534AAA86D7E1055E12936C8A7BD6B865B7DB81 (intptr_t ___0_clazz, intptr_t ___1_fieldID, int16_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticLongField_m299AAC2DE8B6747B0B5E109BABB2F3A4FC1F486E (intptr_t ___0_clazz, intptr_t ___1_fieldID, int64_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticFloatField_mB2EDDE632AB2088CD12F1FD12174FB86990BCBEE (intptr_t ___0_clazz, intptr_t ___1_fieldID, float ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticDoubleField_mA0253927D476917D2158A9CE29F1BF535485B956 (intptr_t ___0_clazz, intptr_t ___1_fieldID, double ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticCharField_m2B8245275C36525798C869B7B1088B25BA663613 (intptr_t ___0_clazz, intptr_t ___1_fieldID, Il2CppChar ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticStringField_m445D977B2374056C6E4607FAEDB7E99A1353E2EE (intptr_t ___0_clazz, intptr_t ___1_fieldID, String_t* ___2_val, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNISafe_SetStaticObjectField_m7757F7E30F8122DAF89F138A8AE727CB896BC721 (intptr_t ___0_clazz, intptr_t ___1_fieldID, intptr_t ___2_val, const RuntimeMethod* method) ;
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* FirebaseHandler_get_AppUtils_m5D80C76317AFA8DBEEFEF2427573A6EE7B6F7B27_inline (const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478 (String_t* ___0_value, const RuntimeMethod* method) ;
inline bool Dictionary_2_TryGetValue_m9DCAC5074133326FBC20B4802AFD2E5761C18D80 (Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* __this, int32_t ___0_key, Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415** ___1_value, const RuntimeMethod* method)
{
	return ((  bool (*) (Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B*, int32_t, Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415**, const RuntimeMethod*))Dictionary_2_TryGetValue_mE84CAE1C2C3C804992294A454700ECB3273CA55C_gshared)(__this, ___0_key, ___1_value, method);
}
inline void Dictionary_2__ctor_m62A668AAEBEB00987A654F7755EFCCEEF5756DD3 (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* __this, const RuntimeMethod* method)
{
	((  void (*) (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415*, const RuntimeMethod*))Dictionary_2__ctor_mE9A1CF09D5006AC41488691817533288065B42FF_gshared)(__this, method);
}
inline void Dictionary_2_set_Item_m97C74ADCBF64325E94417D123CA992CAE1EE3D01 (Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* __this, int32_t ___0_key, Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* ___1_value, const RuntimeMethod* method)
{
	((  void (*) (Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B*, int32_t, Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415*, const RuntimeMethod*))Dictionary_2_set_Item_m2BB6CF3A683DE145DB22935870F3C63F3E8E3027_gshared)(__this, ___0_key, ___1_value, method);
}
inline bool Dictionary_2_TryGetValue_m066DE45CEB9B026DD250BB30F6365406F9FD9EFD (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* __this, String_t* ___0_key, Il2CppSharedGenericObject** ___1_value, const RuntimeMethod* method)
{
	return ((  bool (*) (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415*, String_t*, Il2CppSharedGenericObject**, const RuntimeMethod*))Dictionary_2_TryGetValue_mEB1F53213D9DBB5055E1D48D152CB9B8A5A10437_gshared)(__this, ___0_key, ___1_value, method);
}
inline void Dictionary_2_set_Item_m5B309AB302243C55150F6B418CC3C35D16E908C0 (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* __this, String_t* ___0_key, Il2CppSharedGenericObject* ___1_value, const RuntimeMethod* method)
{
	((  void (*) (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415*, String_t*, Il2CppSharedGenericObject*, const RuntimeMethod*))Dictionary_2_set_Item_m970F39957AA13F57797915183F66166CF35746AD_gshared)(__this, ___0_key, ___1_value, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* __this, String_t* ___0_paramName, const RuntimeMethod* method) ;
inline void ReadOnlyCollection_1__ctor_m010AA1E4CD3820BCC07B77A482D6A17D5C7F14C2 (ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050* __this, RuntimeObject* ___0_list, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050*, RuntimeObject*, const RuntimeMethod*))ReadOnlyCollection_1__ctor_m010AA1E4CD3820BCC07B77A482D6A17D5C7F14C2_gshared)(__this, ___0_list, method);
}
inline void ReadOnlyCollection_1__ctor_mE22404DEC336AFE9817BF78FF9F7F90E07EE4F7A (ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513* __this, RuntimeObject* ___0_list, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513*, RuntimeObject*, const RuntimeMethod*))ReadOnlyCollection_1__ctor_mE22404DEC336AFE9817BF78FF9F7F90E07EE4F7A_gshared)(__this, ___0_list, method);
}
inline void ReadOnlyCollection_1__ctor_mC1890FAC00703F47A655C35CBCB613C74A811580 (ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263* __this, RuntimeObject* ___0_list, const RuntimeMethod* method)
{
	((  void (*) (ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263*, RuntimeObject*, const RuntimeMethod*))ReadOnlyCollection_1__ctor_mC1890FAC00703F47A655C35CBCB613C74A811580_gshared)(__this, ___0_list, method);
}
inline int32_t Array_BinarySearch_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m0AA0BC859F6008F37B680B00508C065B098C7D9A (UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, int32_t ___1_index, int32_t ___2_length, uint64_t ___3_value, RuntimeObject* ___4_comparer, const RuntimeMethod* method)
{
	return ((  int32_t (*) (UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299*, int32_t, int32_t, uint64_t, RuntimeObject*, const RuntimeMethod*))Array_BinarySearch_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m0AA0BC859F6008F37B680B00508C065B098C7D9A_gshared)(___0_array, ___1_index, ___2_length, ___3_value, ___4_comparer, method);
}
inline int32_t Array_BinarySearch_TisSortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53_m8F7822162DB61B62B083C2CD23E5F632B85BF601 (SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B* ___0_array, int32_t ___1_index, int32_t ___2_length, SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 ___3_value, RuntimeObject* ___4_comparer, const RuntimeMethod* method)
{
	return ((  int32_t (*) (SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B*, int32_t, int32_t, SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53, RuntimeObject*, const RuntimeMethod*))Array_BinarySearch_TisSortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53_m8F7822162DB61B62B083C2CD23E5F632B85BF601_gshared)(___0_array, ___1_index, ___2_length, ___3_value, ___4_comparer, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentOutOfRangeException__ctor_mE5B2755F0BEA043CACF915D5CE140859EE58FA66 (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* __this, String_t* ___0_paramName, String_t* ___1_message, const RuntimeMethod* method) ;
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465 (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* __this, String_t* ___0_message, const RuntimeMethod* method) ;
inline ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* ArraySortHelper_1_get_Default_m9357CBDC001EFB2BA8D065940A5C9FEFED3CC036_inline (const RuntimeMethod* method)
{
	return ((  ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* (*) (const RuntimeMethod*))ArraySortHelper_1_get_Default_m9357CBDC001EFB2BA8D065940A5C9FEFED3CC036_gshared_inline)(method);
}
inline int32_t ArraySortHelper_1_BinarySearch_m65877F6FF5C5E15405D3326440C99808F156B993 (ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* __this, CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466* ___0_array, int32_t ___1_index, int32_t ___2_length, CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 ___3_value, RuntimeObject* ___4_comparer, const RuntimeMethod* method)
{
	return ((  int32_t (*) (ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28*, CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466*, int32_t, int32_t, CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565, RuntimeObject*, const RuntimeMethod*))ArraySortHelper_1_BinarySearch_m65877F6FF5C5E15405D3326440C99808F156B993_gshared)(__this, ___0_array, ___1_index, ___2_length, ___3_value, ___4_comparer, method);
}
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56 (const RuntimeMethod* method) ;
// Method Definition Index: 41136
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AllocatorManager_Register_TisIl2CppFullySharedGenericStruct_mF147D5344F44C9D73EE0A38448CDCCBD8931FEA9_gshared (Il2CppFullySharedGenericStruct* ___0_t, bool ___1_IsAutoDispose, bool ___2_isGlobal, int32_t ___3_globalIndex, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AllocatorManager_tFB15A22029C8159A3DCD4C08935BE57D3E6B3C2C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&BurstCompiler_CompileFunctionPointer_TisTryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA_m24AC0F77E9E5A8C61B911FBAF3DC753E0B1FB4DC_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&BurstCompiler_t2715484E1FF256726FC4D4D8E17C35A4C8DFA2B8_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FunctionPointer_1__ctor_mA6464FB1EEC3C76906932127ADC88D71257A9CB6_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	void* L_1 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_23 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_28 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_35 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	//<source_info:<no-source>:1>
	FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C V_0;
	memset((&V_0), 0, sizeof(V_0));
	TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* V_1 = NULL;
	{
		Il2CppFullySharedGenericStruct* L_0 = ___0_t;
		Il2CppConstrainedCallData L_2;
		Il2CppMethodPointer L_3 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_0, &L_2, L_1);
		typedef TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* ( *func_L_4)(void*,const RuntimeMethod*);
		TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* L_5 = ((func_L_4)L_3)(L_2.thisPtr,L_2.method);
		V_1 = L_5;
		TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* L_6 = V_1;
		if (L_6)
		{
			goto IL_001e;
		}
	}
	{
		FunctionPointer_1__ctor_mA6464FB1EEC3C76906932127ADC88D71257A9CB6_inline((&V_0), 0, FunctionPointer_1__ctor_mA6464FB1EEC3C76906932127ADC88D71257A9CB6_RuntimeMethod_var);
		goto IL_0042;
	}

IL_001e:
	{
		TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* L_7 = V_1;
		TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* L_8 = ((AllocatorCache_1_tE4A3FB98A13F6767AD7FC224210A0475AE15E18E_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data_no_init(method->rgctx_data, 4)))->___CachedFunction;
		bool L_9;
		L_9 = Delegate_op_Inequality_mA9EAADBA0C976289CCD49DC5A4BEDBB060B579E0((Delegate_t*)L_7, (Delegate_t*)L_8, NULL);
		if (!L_9)
		{
			goto IL_003c;
		}
	}
	{
		TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* L_10 = V_1;
		il2cpp_codegen_runtime_class_init_inline(BurstCompiler_t2715484E1FF256726FC4D4D8E17C35A4C8DFA2B8_il2cpp_TypeInfo_var);
		FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C L_11;
		L_11 = BurstCompiler_CompileFunctionPointer_TisTryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA_m24AC0F77E9E5A8C61B911FBAF3DC753E0B1FB4DC(L_10, BurstCompiler_CompileFunctionPointer_TisTryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA_m24AC0F77E9E5A8C61B911FBAF3DC753E0B1FB4DC_RuntimeMethod_var);
		((AllocatorCache_1_tE4A3FB98A13F6767AD7FC224210A0475AE15E18E_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data_no_init(method->rgctx_data, 4)))->___TryFunction = L_11;
		TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* L_12 = V_1;
		((AllocatorCache_1_tE4A3FB98A13F6767AD7FC224210A0475AE15E18E_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data_no_init(method->rgctx_data, 4)))->___CachedFunction = L_12;
		Il2CppCodeGenWriteBarrier((void**)(&((AllocatorCache_1_tE4A3FB98A13F6767AD7FC224210A0475AE15E18E_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data_no_init(method->rgctx_data, 4)))->___CachedFunction), (void*)L_12);
	}

IL_003c:
	{
		FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C L_13 = ((AllocatorCache_1_tE4A3FB98A13F6767AD7FC224210A0475AE15E18E_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data_no_init(method->rgctx_data, 4)))->___TryFunction;
		V_0 = L_13;
	}

IL_0042:
	{
		Il2CppFullySharedGenericStruct* L_14 = ___0_t;
		Il2CppFullySharedGenericStruct* L_15 = ___0_t;
		void* L_16;
		L_16 = ((  void* (*) (Il2CppFullySharedGenericStruct*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 5)))(L_15, il2cpp_rgctx_method(method->rgctx_data, 5));
		intptr_t L_17;
		L_17 = IntPtr_op_Explicit_mE2CEC14C61FD5E2159A03EA2AD97F5CDC5BB9F4D(L_16, NULL);
		FunctionPointer_1_tF99F1F7D7E9F1AC1CB5F7DE7BB02E8366FC2097C L_18 = V_0;
		bool L_19 = ___1_IsAutoDispose;
		bool L_20 = ___2_isGlobal;
		int32_t L_21 = ___3_globalIndex;
		il2cpp_codegen_runtime_class_init_inline(AllocatorManager_tFB15A22029C8159A3DCD4C08935BE57D3E6B3C2C_il2cpp_TypeInfo_var);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_22;
		L_22 = AllocatorManager_Register_mCA48F5A5C481281C97CA4E0D1E934CB771D00237(L_17, L_18, L_19, L_20, L_21, NULL);
		Il2CppConstrainedCallData L_24;
		Il2CppMethodPointer L_25 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 6), (void*)L_14, &L_24, L_23);
		typedef void ( *func_L_26)(void*,AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148,const RuntimeMethod*);
		((func_L_26)L_25)(L_24.thisPtr, L_22,L_24.method);
		Il2CppFullySharedGenericStruct* L_27 = ___0_t;
		Il2CppConstrainedCallData L_29;
		Il2CppMethodPointer L_30 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 7), (void*)L_27, &L_29, L_28);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_31)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_32 = ((func_L_31)L_30)(L_29.thisPtr,L_29.method);
		uint16_t L_33 = L_32.___Index;
		Il2CppFullySharedGenericStruct* L_34 = ___0_t;
		Il2CppConstrainedCallData L_36;
		Il2CppMethodPointer L_37 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_34, &L_36, L_35);
		typedef TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* ( *func_L_38)(void*,const RuntimeMethod*);
		TryFunction_tC277E17D1D6AD4EF20C1FC1D8F91A559208AA5CA* L_39 = ((func_L_38)L_37)(L_36.thisPtr,L_36.method);
		il2cpp_codegen_runtime_class_init_inline(Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		Managed_RegisterDelegate_m494ED81F0C1945174DD1E82D6711EB329255E36F((int32_t)L_33, L_39, NULL);
		return;
	}
}
// Method Definition Index: 41137
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AllocatorManager_UnmanagedUnregister_TisIl2CppFullySharedGenericStruct_mA531B69E183CA56C8BC7D2DD419333CA5E2DC004_gshared (Il2CppFullySharedGenericStruct* ___0_t, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	void* L_1 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_8 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_16 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_25 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	//<source_info:<no-source>:1>
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 V_0;
	memset((&V_0), 0, sizeof(V_0));
	TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE V_1;
	memset((&V_1), 0, sizeof(V_1));
	{
		Il2CppFullySharedGenericStruct* L_0 = ___0_t;
		Il2CppConstrainedCallData L_2;
		Il2CppMethodPointer L_3 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_0, &L_2, L_1);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_4)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_5 = ((func_L_4)L_3)(L_2.thisPtr,L_2.method);
		V_0 = L_5;
		bool L_6;
		L_6 = AllocatorHandle_get_IsInstalled_mB38CD887177A87128DC9A2DE6F866F9EC18FA907((&V_0), NULL);
		if (!L_6)
		{
			goto IL_007d;
		}
	}
	{
		Il2CppFullySharedGenericStruct* L_7 = ___0_t;
		Il2CppConstrainedCallData L_9;
		Il2CppMethodPointer L_10 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_7, &L_9, L_8);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_11)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_12 = ((func_L_11)L_10)(L_9.thisPtr,L_9.method);
		V_0 = L_12;
		il2cpp_codegen_initobj((&V_1), sizeof(TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE));
		TableEntry_t5E44AFA7857A41AC654D7F248FD36B15D7835FFE L_13 = V_1;
		AllocatorHandle_Install_m0526A06766A02754698DE0115B926C15566CDD3B((&V_0), L_13, NULL);
		il2cpp_codegen_runtime_class_init_inline(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_14;
		L_14 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_StaticFields*)il2cpp_codegen_static_fields_for(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		Il2CppFullySharedGenericStruct* L_15 = ___0_t;
		Il2CppConstrainedCallData L_17;
		Il2CppMethodPointer L_18 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_15, &L_17, L_16);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_19)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_20 = ((func_L_19)L_18)(L_17.thisPtr,L_17.method);
		V_0 = L_20;
		int32_t L_21;
		L_21 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_22;
		L_22 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_14, L_21, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_runtime_class_init_inline(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_23;
		L_23 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_StaticFields*)il2cpp_codegen_static_fields_for(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		Il2CppFullySharedGenericStruct* L_24 = ___0_t;
		Il2CppConstrainedCallData L_26;
		Il2CppMethodPointer L_27 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_24, &L_26, L_25);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_28)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_29 = ((func_L_28)L_27)(L_26.thisPtr,L_26.method);
		V_0 = L_29;
		int32_t L_30;
		L_30 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_31;
		L_31 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_23, L_30, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
	}

IL_007d:
	{
		return;
	}
}
// Method Definition Index: 41138
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AllocatorManager_Unregister_TisRewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254_m9A4CBB4B25F9752F36118A0658985C25F8FADB1A_gshared (RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* ___0_t, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	//<source_info:<no-source>:1>
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* L_0 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_1;
		L_1 = RewindableAllocator_get_Handle_mF81EDA2102485C46965AAB56347E8F64FE551D9E_inline(L_0, NULL);
		V_0 = L_1;
		bool L_2;
		L_2 = AllocatorHandle_get_IsInstalled_mB38CD887177A87128DC9A2DE6F866F9EC18FA907((&V_0), NULL);
		if (!L_2)
		{
			goto IL_008a;
		}
	}
	{
		RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* L_3 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4;
		L_4 = RewindableAllocator_get_Handle_mF81EDA2102485C46965AAB56347E8F64FE551D9E_inline(L_3, NULL);
		V_0 = L_4;
		AllocatorHandle_Dispose_mB74CBC8980962C016A6C85F09D3F94775A2C58E3((&V_0), NULL);
		il2cpp_codegen_runtime_class_init_inline(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_5;
		L_5 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_StaticFields*)il2cpp_codegen_static_fields_for(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* L_6 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_7;
		L_7 = RewindableAllocator_get_Handle_mF81EDA2102485C46965AAB56347E8F64FE551D9E_inline(L_6, NULL);
		V_0 = L_7;
		int32_t L_8;
		L_8 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_9;
		L_9 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_5, L_8, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_runtime_class_init_inline(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_10;
		L_10 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_StaticFields*)il2cpp_codegen_static_fields_for(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* L_11 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_12;
		L_12 = RewindableAllocator_get_Handle_mF81EDA2102485C46965AAB56347E8F64FE551D9E_inline(L_11, NULL);
		V_0 = L_12;
		int32_t L_13;
		L_13 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_14;
		L_14 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_10, L_13, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* L_15 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_16;
		L_16 = RewindableAllocator_get_Handle_mF81EDA2102485C46965AAB56347E8F64FE551D9E_inline(L_15, NULL);
		uint16_t L_17 = L_16.___Index;
		il2cpp_codegen_runtime_class_init_inline(Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		Managed_UnregisterDelegate_mE60C615D7705C878A66FB0AA06C7EE86A2359D46((int32_t)L_17, NULL);
	}

IL_008a:
	{
		return;
	}
}
// Method Definition Index: 41138
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AllocatorManager_Unregister_TisIl2CppFullySharedGenericStruct_m1782C417767DBDCC0F33DE0B0ACCEE7BE0C9570D_gshared (Il2CppFullySharedGenericStruct* ___0_t, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		il2cpp_rgctx_method_init(method);
	}
	void* L_1 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_8 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_15 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_24 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	void* L_32 = alloca(Il2CppFakeBoxBuffer::SizeNeededFor(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)));
	//<source_info:<no-source>:1>
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		Il2CppFullySharedGenericStruct* L_0 = ___0_t;
		Il2CppConstrainedCallData L_2;
		Il2CppMethodPointer L_3 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_0, &L_2, L_1);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_4)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_5 = ((func_L_4)L_3)(L_2.thisPtr,L_2.method);
		V_0 = L_5;
		bool L_6;
		L_6 = AllocatorHandle_get_IsInstalled_mB38CD887177A87128DC9A2DE6F866F9EC18FA907((&V_0), NULL);
		if (!L_6)
		{
			goto IL_008a;
		}
	}
	{
		Il2CppFullySharedGenericStruct* L_7 = ___0_t;
		Il2CppConstrainedCallData L_9;
		Il2CppMethodPointer L_10 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_7, &L_9, L_8);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_11)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_12 = ((func_L_11)L_10)(L_9.thisPtr,L_9.method);
		V_0 = L_12;
		AllocatorHandle_Dispose_mB74CBC8980962C016A6C85F09D3F94775A2C58E3((&V_0), NULL);
		il2cpp_codegen_runtime_class_init_inline(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_13;
		L_13 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_StaticFields*)il2cpp_codegen_static_fields_for(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		Il2CppFullySharedGenericStruct* L_14 = ___0_t;
		Il2CppConstrainedCallData L_16;
		Il2CppMethodPointer L_17 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_14, &L_16, L_15);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_18)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_19 = ((func_L_18)L_17)(L_16.thisPtr,L_16.method);
		V_0 = L_19;
		int32_t L_20;
		L_20 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_21;
		L_21 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_13, L_20, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_runtime_class_init_inline(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_22;
		L_22 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_StaticFields*)il2cpp_codegen_static_fields_for(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		Il2CppFullySharedGenericStruct* L_23 = ___0_t;
		Il2CppConstrainedCallData L_25;
		Il2CppMethodPointer L_26 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_23, &L_25, L_24);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_27)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_28 = ((func_L_27)L_26)(L_25.thisPtr,L_25.method);
		V_0 = L_28;
		int32_t L_29;
		L_29 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_30;
		L_30 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_22, L_29, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		Il2CppFullySharedGenericStruct* L_31 = ___0_t;
		Il2CppConstrainedCallData L_33;
		Il2CppMethodPointer L_34 = il2cpp_codegen_get_runtime_constrained_call_data(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), il2cpp_rgctx_method(method->rgctx_data, 2), (void*)L_31, &L_33, L_32);
		typedef AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 ( *func_L_35)(void*,const RuntimeMethod*);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_36 = ((func_L_35)L_34)(L_33.thisPtr,L_33.method);
		uint16_t L_37 = L_36.___Index;
		il2cpp_codegen_runtime_class_init_inline(Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		Managed_UnregisterDelegate_mE60C615D7705C878A66FB0AA06C7EE86A2359D46((int32_t)L_37, NULL);
	}

IL_008a:
	{
		return;
	}
}
// Method Definition Index: 41138
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AllocatorManager_Unregister_TisAllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148_mB8321A6BD18AF456ABC30E251B65EDA0381ECE93_gshared (AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* ___0_t, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		s_Il2CppMethodInitialized = true;
	}
	//<source_info:<no-source>:1>
	AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* L_0 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_1;
		L_1 = AllocatorHandle_get_Handle_m440EA9B9A4306115087775DA2AA0AC034107D0E2(L_0, NULL);
		V_0 = L_1;
		bool L_2;
		L_2 = AllocatorHandle_get_IsInstalled_mB38CD887177A87128DC9A2DE6F866F9EC18FA907((&V_0), NULL);
		if (!L_2)
		{
			goto IL_008a;
		}
	}
	{
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* L_3 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_4;
		L_4 = AllocatorHandle_get_Handle_m440EA9B9A4306115087775DA2AA0AC034107D0E2(L_3, NULL);
		V_0 = L_4;
		AllocatorHandle_Dispose_mB74CBC8980962C016A6C85F09D3F94775A2C58E3((&V_0), NULL);
		il2cpp_codegen_runtime_class_init_inline(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_5;
		L_5 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_StaticFields*)il2cpp_codegen_static_fields_for(IsInstalled_t42C3CF2626E162EF686994F983A03FAACB65537C_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* L_6 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_7;
		L_7 = AllocatorHandle_get_Handle_m440EA9B9A4306115087775DA2AA0AC034107D0E2(L_6, NULL);
		V_0 = L_7;
		int32_t L_8;
		L_8 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_9;
		L_9 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_5, L_8, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		il2cpp_codegen_runtime_class_init_inline(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var);
		Long1024_tEE887C506947419DC829213E6C7483D80AF5659F* L_10;
		L_10 = SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545((&((IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_StaticFields*)il2cpp_codegen_static_fields_for(IsAutoDispose_t042DBFFA97CC00077DEB1A53BB0046B580CFE27F_il2cpp_TypeInfo_var))->___Ref), SharedStatic_1_get_Data_m3D45E4A5D48C36523EAEE679ED21687C5F4DC545_RuntimeMethod_var);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* L_11 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_12;
		L_12 = AllocatorHandle_get_Handle_m440EA9B9A4306115087775DA2AA0AC034107D0E2(L_11, NULL);
		V_0 = L_12;
		int32_t L_13;
		L_13 = AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline((&V_0), NULL);
		int32_t L_14;
		L_14 = ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323(L_10, L_13, 1, ConcurrentMask_TryFree_TisLong1024_tEE887C506947419DC829213E6C7483D80AF5659F_mA73898672B3B82A8A0B3D5FB2654735189181323_RuntimeMethod_var);
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* L_15 = ___0_t;
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_16;
		L_16 = AllocatorHandle_get_Handle_m440EA9B9A4306115087775DA2AA0AC034107D0E2(L_15, NULL);
		uint16_t L_17 = L_16.___Index;
		il2cpp_codegen_runtime_class_init_inline(Managed_t7CB1B315B8E0E50EE8A2993B3E4CDF35E2B4909D_il2cpp_TypeInfo_var);
		Managed_UnregisterDelegate_mE60C615D7705C878A66FB0AA06C7EE86A2359D46((int32_t)L_17, NULL);
	}

IL_008a:
	{
		return;
	}
}
// Method Definition Index: 66416
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidImpl_CallInternalMethod_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m0D7D8DBEF7844E65BDAAA04044FA857D2525D7A5_gshared (AndroidImpl_t09BB72854905028A1DF3FBA8772392723D2CCD76* __this, Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457* ___0_methodCall, String_t* ___1_operation, bool ___2_errorValue, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	RuntimeObject* V_0 = NULL;
	bool V_1 = false;
	bool V_2 = false;
	bool V_3 = false;
	int32_t G_B4_0 = 0;
	{
		il2cpp_codegen_runtime_class_init_inline(FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var);
		RuntimeObject* L_0 = ((FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_StaticFields*)il2cpp_codegen_static_fields_for(FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var))->___disposeLock;
		V_0 = L_0;
		V_1 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_003d:
			{
				{
					bool L_1 = V_1;
					if (!L_1)
					{
						goto IL_0047;
					}
				}
				{
					RuntimeObject* L_2 = V_0;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA(L_2, NULL);
				}

IL_0047:
				{
					return;
				}
			}
		});
		try
		{
			{
				RuntimeObject* L_3 = V_0;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149(L_3, (&V_1), NULL);
				FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40* L_4 = __this->___crashlyticsInternal;
				if (!L_4)
				{
					goto IL_002b_1;
				}
			}
			{
				FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40* L_5 = __this->___crashlyticsInternal;
				NullCheck(L_5);
				bool L_6;
				L_6 = FirebaseCrashlyticsInternal_get_IsDisposed_m47E32597ACDF3FAC963438574982F9D43965EEB2(L_5, NULL);
				G_B4_0 = ((((int32_t)L_6) == ((int32_t)0))? 1 : 0);
				goto IL_002c_1;
			}

IL_002b_1:
			{
				G_B4_0 = 0;
			}

IL_002c_1:
			{
				V_2 = (bool)G_B4_0;
				bool L_7 = V_2;
				if (!L_7)
				{
					goto IL_003a_1;
				}
			}
			{
				Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457* L_8 = ___0_methodCall;
				NullCheck(L_8);
				bool L_9;
				L_9 = Func_1_Invoke_mBB7F37C468451AF57FAF31635C544D6B8C4373B2_inline(L_8, il2cpp_rgctx_method(method->rgctx_data, 1));
				V_3 = L_9;
				goto IL_0054;
			}

IL_003a_1:
			{
				goto IL_0048;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0048:
	{
		String_t* L_10 = ___1_operation;
		AndroidImpl_LogOperationFailedWarningDueToShutdown_m7FF1B7FA100D2AA4AE5D871CB6845FA5B0FE0FCF(__this, L_10, NULL);
		bool L_11 = ___2_errorValue;
		V_3 = L_11;
		goto IL_0054;
	}

IL_0054:
	{
		bool L_12 = V_3;
		return L_12;
	}
}
// Method Definition Index: 66416
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidImpl_CallInternalMethod_TisIl2CppFullySharedGenericAny_m463A2374AB05EE186970EEE463023AB0F7CF6B6F_gshared (AndroidImpl_t09BB72854905028A1DF3FBA8772392723D2CCD76* __this, Func_1_tBB8824FA8746333BFFF3AB3CE4A41B58450AF431* ___0_methodCall, String_t* ___1_operation, Il2CppFullySharedGenericAny ___2_errorValue, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_9 = alloca(SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
	const Il2CppFullySharedGenericAny L_11 = L_9;
	const Il2CppFullySharedGenericAny L_12 = L_9;
	//<source_info:<no-source>:1>
	RuntimeObject* V_0 = NULL;
	bool V_1 = false;
	bool V_2 = false;
	Il2CppFullySharedGenericAny V_3 = alloca(SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
	memset(V_3, 0, SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
	int32_t G_B4_0 = 0;
	{
		il2cpp_codegen_runtime_class_init_inline(FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var);
		RuntimeObject* L_0 = ((FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_StaticFields*)il2cpp_codegen_static_fields_for(FirebaseApp_tD23C437863A3502177988D1382B58820B0571A25_il2cpp_TypeInfo_var))->___disposeLock;
		V_0 = L_0;
		V_1 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_003d:
			{
				{
					bool L_1 = V_1;
					if (!L_1)
					{
						goto IL_0047;
					}
				}
				{
					RuntimeObject* L_2 = V_0;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA(L_2, NULL);
				}

IL_0047:
				{
					return;
				}
			}
		});
		try
		{
			{
				RuntimeObject* L_3 = V_0;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149(L_3, (&V_1), NULL);
				FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40* L_4 = __this->___crashlyticsInternal;
				if (!L_4)
				{
					goto IL_002b_1;
				}
			}
			{
				FirebaseCrashlyticsInternal_t674756D0479C8ED14FE9E114B533CD274F9D6F40* L_5 = __this->___crashlyticsInternal;
				NullCheck(L_5);
				bool L_6;
				L_6 = FirebaseCrashlyticsInternal_get_IsDisposed_m47E32597ACDF3FAC963438574982F9D43965EEB2(L_5, NULL);
				G_B4_0 = ((((int32_t)L_6) == ((int32_t)0))? 1 : 0);
				goto IL_002c_1;
			}

IL_002b_1:
			{
				G_B4_0 = 0;
			}

IL_002c_1:
			{
				V_2 = (bool)G_B4_0;
				bool L_7 = V_2;
				if (!L_7)
				{
					goto IL_003a_1;
				}
			}
			{
				Func_1_tBB8824FA8746333BFFF3AB3CE4A41B58450AF431* L_8 = ___0_methodCall;
				NullCheck(L_8);
				InvokerActionInvoker1< Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), L_8, (Il2CppFullySharedGenericAny*)L_9);
				il2cpp_codegen_memcpy(V_3, L_9, SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
				goto IL_0054;
			}

IL_003a_1:
			{
				goto IL_0048;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0048:
	{
		String_t* L_10 = ___1_operation;
		AndroidImpl_LogOperationFailedWarningDueToShutdown_m7FF1B7FA100D2AA4AE5D871CB6845FA5B0FE0FCF(__this, L_10, NULL);
		il2cpp_codegen_memcpy(L_11, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 2)) ? ___2_errorValue : &___2_errorValue), SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
		il2cpp_codegen_memcpy(V_3, L_11, SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
		goto IL_0054;
	}

IL_0054:
	{
		il2cpp_codegen_memcpy(L_12, V_3, SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
		il2cpp_codegen_memcpy(il2cppRetVal, L_12, SizeOf_T_tF2471ECA74C0E57E3A3D42AB69D0E867326BFC66);
		return;
	}
}
// Method Definition Index: 62865
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF AndroidJNI_GetDirectBuffer_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m971CFD6732DCF55F07C2820EA7F2D685FED934CD_gshared (intptr_t ___0_buffer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int8_t* V_0 = NULL;
	int64_t V_1 = 0;
	NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		intptr_t L_0 = ___0_buffer;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_2), sizeof(NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF));
		NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF L_2 = V_2;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_buffer;
		int8_t* L_4;
		L_4 = AndroidJNI_GetDirectBufferAddress_m0E0B127BFEB7AAF065829DC6AE11163D5616EBE8(L_3, NULL);
		V_0 = L_4;
		int8_t* L_5 = V_0;
		uintptr_t L_6 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(0,NULL));
		if ((!(((uintptr_t)L_5) == ((uintptr_t)L_6))))
		{
			goto IL_002d;
		}
	}
	{
		il2cpp_codegen_initobj((&V_2), sizeof(NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF));
		NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF L_7 = V_2;
		return L_7;
	}

IL_002d:
	{
		intptr_t L_8 = ___0_buffer;
		int64_t L_9;
		L_9 = AndroidJNI_GetDirectBufferCapacity_mCAC8D9C8E45481BE59FB17406E1E16D4F9628183(L_8, NULL);
		V_1 = L_9;
		int64_t L_10 = V_1;
		int64_t L_11 = (il2cpp_codegen_conv<int64_t,int32_t,int32_t,false,false>(((int32_t)2147483647LL),NULL));
		if ((((int64_t)L_10) <= ((int64_t)L_11)))
		{
			goto IL_005d;
		}
	}
	{
		int64_t L_12 = V_1;
		int64_t L_13 = L_12;
		RuntimeObject* L_14 = Box(il2cpp_defaults.int64_class, &L_13);
		int32_t L_15 = ((int32_t)2147483647LL);
		RuntimeObject* L_16 = Box(il2cpp_defaults.int32_class, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralDC955F06F1DA8A3B06C4755211822B06D698CF11)), L_14, L_16, NULL);
		Exception_t* L_18 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_18, L_17, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_18, method);
	}

IL_005d:
	{
		int8_t* L_19 = V_0;
		int64_t L_20 = V_1;
		int32_t L_21 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>(L_20,NULL));
		NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF L_22;
		L_22 = NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m181D7F12EB826B7D6B73742BFD85A667D533BABA((void*)L_19, L_21, (int32_t)1, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_22;
	}
}
// Method Definition Index: 62865
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 AndroidJNI_GetDirectBuffer_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mD58C4C3DF41E0AFB0378C0FF602335B30319D382_gshared (intptr_t ___0_buffer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int8_t* V_0 = NULL;
	int64_t V_1 = 0;
	NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		intptr_t L_0 = ___0_buffer;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_2), sizeof(NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190));
		NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 L_2 = V_2;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_buffer;
		int8_t* L_4;
		L_4 = AndroidJNI_GetDirectBufferAddress_m0E0B127BFEB7AAF065829DC6AE11163D5616EBE8(L_3, NULL);
		V_0 = L_4;
		int8_t* L_5 = V_0;
		uintptr_t L_6 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(0,NULL));
		if ((!(((uintptr_t)L_5) == ((uintptr_t)L_6))))
		{
			goto IL_002d;
		}
	}
	{
		il2cpp_codegen_initobj((&V_2), sizeof(NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190));
		NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 L_7 = V_2;
		return L_7;
	}

IL_002d:
	{
		intptr_t L_8 = ___0_buffer;
		int64_t L_9;
		L_9 = AndroidJNI_GetDirectBufferCapacity_mCAC8D9C8E45481BE59FB17406E1E16D4F9628183(L_8, NULL);
		V_1 = L_9;
		int64_t L_10 = V_1;
		int64_t L_11 = (il2cpp_codegen_conv<int64_t,int32_t,int32_t,false,false>(((int32_t)2147483647LL),NULL));
		if ((((int64_t)L_10) <= ((int64_t)L_11)))
		{
			goto IL_005d;
		}
	}
	{
		int64_t L_12 = V_1;
		int64_t L_13 = L_12;
		RuntimeObject* L_14 = Box(il2cpp_defaults.int64_class, &L_13);
		int32_t L_15 = ((int32_t)2147483647LL);
		RuntimeObject* L_16 = Box(il2cpp_defaults.int32_class, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralDC955F06F1DA8A3B06C4755211822B06D698CF11)), L_14, L_16, NULL);
		Exception_t* L_18 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_18, L_17, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_18, method);
	}

IL_005d:
	{
		int8_t* L_19 = V_0;
		int64_t L_20 = V_1;
		int32_t L_21 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>(L_20,NULL));
		NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 L_22;
		L_22 = NativeArrayUnsafeUtility_ConvertExistingDataToNativeArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mD9D29EE5CE27F4BFD0168818BF064EC5B3672F39((void*)L_19, L_21, (int32_t)1, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_22;
	}
}
// Method Definition Index: 62865
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 AndroidJNI_GetDirectBuffer_TisIl2CppFullySharedGenericStruct_mA240D97969B6DC457DEC6F6EB97FDE5D94F28C29_gshared (intptr_t ___0_buffer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int8_t* V_0 = NULL;
	int64_t V_1 = 0;
	NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 V_2;
	memset((&V_2), 0, sizeof(V_2));
	{
		intptr_t L_0 = ___0_buffer;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_2), sizeof(NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18));
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_2 = V_2;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_buffer;
		int8_t* L_4;
		L_4 = AndroidJNI_GetDirectBufferAddress_m0E0B127BFEB7AAF065829DC6AE11163D5616EBE8(L_3, NULL);
		V_0 = L_4;
		int8_t* L_5 = V_0;
		uintptr_t L_6 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(0,NULL));
		if ((!(((uintptr_t)L_5) == ((uintptr_t)L_6))))
		{
			goto IL_002d;
		}
	}
	{
		il2cpp_codegen_initobj((&V_2), sizeof(NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18));
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_7 = V_2;
		return L_7;
	}

IL_002d:
	{
		intptr_t L_8 = ___0_buffer;
		int64_t L_9;
		L_9 = AndroidJNI_GetDirectBufferCapacity_mCAC8D9C8E45481BE59FB17406E1E16D4F9628183(L_8, NULL);
		V_1 = L_9;
		int64_t L_10 = V_1;
		int64_t L_11 = (il2cpp_codegen_conv<int64_t,int32_t,int32_t,false,false>(((int32_t)2147483647LL),NULL));
		if ((((int64_t)L_10) <= ((int64_t)L_11)))
		{
			goto IL_005d;
		}
	}
	{
		int64_t L_12 = V_1;
		int64_t L_13 = L_12;
		RuntimeObject* L_14 = Box(il2cpp_defaults.int64_class, &L_13);
		int32_t L_15 = ((int32_t)2147483647LL);
		RuntimeObject* L_16 = Box(il2cpp_defaults.int32_class, &L_15);
		String_t* L_17;
		L_17 = String_Format_mFB7DA489BD99F4670881FF50EC017BFB0A5C0987(((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralDC955F06F1DA8A3B06C4755211822B06D698CF11)), L_14, L_16, NULL);
		Exception_t* L_18 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_18, L_17, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_18, method);
	}

IL_005d:
	{
		int8_t* L_19 = V_0;
		int64_t L_20 = V_1;
		int32_t L_21 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>(L_20,NULL));
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_22;
		L_22 = ((  NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 (*) (void*, int32_t, int32_t, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((void*)L_19, L_21, (int32_t)1, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_22;
	}
}
// Method Definition Index: 62862
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNI_NewDirectByteBufferFromNativeArray_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m5CA9D512889E4FD11E06BC83A1E6FE316CD1BC0F_gshared (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF ___0_buffer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		bool L_0;
		L_0 = NativeArray_1_get_IsCreated_mD74FCA194584E6EA7916853B62401EB78240A081_inline((&___0_buffer), il2cpp_rgctx_method(method->rgctx_data, 1));
		if (!L_0)
		{
			goto IL_0013;
		}
	}
	{
		int32_t L_1;
		L_1 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___0_buffer))->___m_Length);
		if ((((int32_t)L_1) > ((int32_t)0)))
		{
			goto IL_0019;
		}
	}

IL_0013:
	{
		return 0;
	}

IL_0019:
	{
		NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF L_2 = ___0_buffer;
		void* L_3;
		L_3 = NativeArrayUnsafeUtility_GetUnsafePtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m8CFDB2DF56E810A2E2FB3686AF676FCAC65AFCC2_inline(L_2, il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___0_buffer))->___m_Length);
		int64_t L_5 = (il2cpp_codegen_conv<int64_t,int32_t,int32_t,false,false>(L_4,NULL));
		intptr_t L_6;
		L_6 = AndroidJNI_NewDirectByteBuffer_m17389ED6D98CC0364180BAB43F2747B48FFDB107((uint8_t*)L_3, L_5, NULL);
		return L_6;
	}
}
// Method Definition Index: 62862
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNI_NewDirectByteBufferFromNativeArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mB9A49EBBCCE6CE5AF7544594285D5F56E7EF279C_gshared (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 ___0_buffer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		bool L_0;
		L_0 = NativeArray_1_get_IsCreated_mF1154ABCB79F43B3A71BFA8DCB85ACBDFA2838D5_inline((&___0_buffer), il2cpp_rgctx_method(method->rgctx_data, 1));
		if (!L_0)
		{
			goto IL_0013;
		}
	}
	{
		int32_t L_1;
		L_1 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___0_buffer))->___m_Length);
		if ((((int32_t)L_1) > ((int32_t)0)))
		{
			goto IL_0019;
		}
	}

IL_0013:
	{
		return 0;
	}

IL_0019:
	{
		NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 L_2 = ___0_buffer;
		void* L_3;
		L_3 = NativeArrayUnsafeUtility_GetUnsafePtr_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mCD832837FF5CE235B506272835BA049D56968E2F_inline(L_2, il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_4;
		L_4 = IL2CPP_NATIVEARRAY_GET_LENGTH(((&___0_buffer))->___m_Length);
		int64_t L_5 = (il2cpp_codegen_conv<int64_t,int32_t,int32_t,false,false>(L_4,NULL));
		intptr_t L_6;
		L_6 = AndroidJNI_NewDirectByteBuffer_m17389ED6D98CC0364180BAB43F2747B48FFDB107((uint8_t*)L_3, L_5, NULL);
		return L_6;
	}
}
// Method Definition Index: 62862
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNI_NewDirectByteBufferFromNativeArray_TisIl2CppFullySharedGenericStruct_mBA315BD28FAF24143A02065102FEC37584F134D1_gshared (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 ___0_buffer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		bool L_0;
		L_0 = ((  bool (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)))((&___0_buffer), il2cpp_rgctx_method(method->rgctx_data, 1));
		if (!L_0)
		{
			goto IL_0013;
		}
	}
	{
		int32_t L_1;
		L_1 = ((  int32_t (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)))((&___0_buffer), il2cpp_rgctx_method(method->rgctx_data, 3));
		if ((((int32_t)L_1) > ((int32_t)0)))
		{
			goto IL_0019;
		}
	}

IL_0013:
	{
		return 0;
	}

IL_0019:
	{
		NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18 L_2 = ___0_buffer;
		void* L_3;
		L_3 = ((  void* (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 4)))(L_2, il2cpp_rgctx_method(method->rgctx_data, 4));
		int32_t L_4;
		L_4 = ((  int32_t (*) (NativeArray_1_tDB8B8DC66CC8E16ED6D9A8C75D2C1AFC80AC1E18*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)))((&___0_buffer), il2cpp_rgctx_method(method->rgctx_data, 3));
		int64_t L_5 = (il2cpp_codegen_conv<int64_t,int32_t,int32_t,false,false>(L_4,NULL));
		intptr_t L_6;
		L_6 = AndroidJNI_NewDirectByteBuffer_m17389ED6D98CC0364180BAB43F2747B48FFDB107((uint8_t*)L_3, L_5, NULL);
		return L_6;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m0207DDD0AB7EE1136083B85289D4F16FD9A64ECC_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		bool L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mBF1AB84DE0B50E7C206BAD3C170D1F5AD2D9E343(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m51DC292506277213F541434A51F81430E11E7226_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		Il2CppChar L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m9ECB51B699AB7B96368F39CC9F7C481340DD0BBC(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mF6AF89E7365D44A201C5A2C86C49EF49E2D6ADAD_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		double L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m1F7F249CC7CDA2D6E136C627D983569B3674EA73(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF6FCB78A4CF7C2FB0375E88C31C70458F55F422C_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		int16_t L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m901F767E3189385BB47B5B021C40507AAF67D477(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m02707C23958FF03FA0345C2A17F66EE379A1468C_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		int32_t L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m37A94A4CA9F6456009CABE27D4AA2ED7B8365168(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m0CADE835010213386C1C07E63F70E55498142C20_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		int64_t L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mBE697F1C4D60A7F2BB06784B5CB0C540B7FBF664(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m2BD2E924FBD117B44A2D5E741144FD7B1FCE918E_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		int8_t L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m01777984AFB915C1167148BC6056336EB200B151(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m93BCF3156F5C823F8C2B005523A618966B0A6DBB_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		float L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6634501B2BC58937A4D852F6F2D8D55A9DFB6F64(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_m6F1CD10E4BE61A7211FB87DA1D7D92C7FE85F8E4_gshared (intptr_t ___0_array, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		Il2CppSharedGenericObject* L_1;
		L_1 = _AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_mAD2432704589B67F3F93D55DF8B948DCBEBA4444(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 62607
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppFullySharedGenericAny_mE2FE04F1AE05E31AFCB6C62A0D8015274310BBE7_gshared (intptr_t ___0_array, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_ArrayType_t33FA9A7F66F041B4E2878FF619DA2A8FCDD39085 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_ArrayType_t33FA9A7F66F041B4E2878FF619DA2A8FCDD39085);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_array;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), NULL, L_0, (Il2CppFullySharedGenericAny*)L_1);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_ArrayType_t33FA9A7F66F041B4E2878FF619DA2A8FCDD39085);
		return;
	}
}
// Method Definition Index: 62609
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1B394DCE5ED40DFA09EB18874BA2FFC3B5E50B2E_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_fieldName;
		bool L_2 = ___2_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_3;
		L_3 = _AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mFD988C7487D7A7810D33F985E48AB82A006E1B7B(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_3;
	}
}
// Method Definition Index: 62609
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mDD609864F766F77FB093EC507721536574D38177_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_fieldName;
		bool L_2 = ___2_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_3;
		L_3 = _AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m646B71E5782A91A5A247FFAEEA04D44FB31AA619(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_3;
	}
}
// Method Definition Index: 62609
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mDFF491584A820BE0629A47ED3B790A199050D589_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_fieldName;
		bool L_2 = ___2_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_3;
		L_3 = _AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mC4E70171E726BFD2EDF128963FAD90873B0289C5(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_3;
	}
}
// Method Definition Index: 62609
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetFieldID_TisIl2CppFullySharedGenericAny_mD4DFA0128576804E9AE4FF17BD3D1398F2196069_gshared (intptr_t ___0_jclass, String_t* ___1_fieldName, bool ___2_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_fieldName;
		bool L_2 = ___2_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_3;
		L_3 = ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_0, L_1, L_2, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_3;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mE3914411BE312CBBA869835158E535A06CD3EEB4_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m5C5661781E8C49E962C2556FC75101FD0E862AFD(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m791647AA6EAD3B88467418F51CF53F5755AB38BA_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m465627642B00BC71088A5883058BB86DDBADABE0(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m44C60AAE64EE97BFD5D9D302E7BE5BE20E6862B3_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mA32D3320555971B585058A28FC5F5EC84DC10A34(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m3B57915B048BD436E00741CD146D62AC775E7BA3_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF0D2DB5B885B01587820A9F88C6CAD67C8DC1512(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mF87CC238DDBB3C90C35846E626548847CC7E319D_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB340108BAAC2DA25B45971B719093FDF9B13E745(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m9665934CC08FD80EB16882C4EA6FDBC5D9C3A175_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mF114D8205E13517DE4F8DB9772E9C40C7C43BD85(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mEF6ACD1C6C5237E0446DB9DBDAC40BA2660849BB_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m362BBB1CC3B8834A056A8F709649BD182E24E9BD(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mA3FFDC6948922BEFC787A297D5483BA277EEFF0C_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m86B190CF71EDCB08177B90F98C76A9545AF09FCB(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_m95122CB55DFAF463B14CBBA51C2B0811790DB90B_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = _AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_mFEAB91A44B644CE56BB6182931D1065B679E0B3A(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62608
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR intptr_t AndroidJNIHelper_GetMethodID_TisIl2CppFullySharedGenericAny_mEB124826B537513D20B0E11A265E04D0612E1CE8_gshared (intptr_t ___0_jclass, String_t* ___1_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___2_args, bool ___3_isStatic, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_jclass;
		String_t* L_1 = ___1_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___2_args;
		bool L_3 = ___3_isStatic;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		intptr_t L_4;
		L_4 = ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_0, L_1, L_2, L_3, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_4;
	}
}
// Method Definition Index: 62610
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR String_t* AndroidJNIHelper_GetSignature_TisIl2CppFullySharedGenericAny_m4D04F21DE5CB13BC4E2A08505DA57AC729F3BA70_gshared (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___0_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___0_args;
		il2cpp_codegen_runtime_class_init_inline(_AndroidJNIHelper_tA796944DDB1B1459DF68C9FFA518F452C81364F3_il2cpp_TypeInfo_var);
		String_t* L_1;
		L_1 = ((  String_t* (*) (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject_Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m05D3284A3FA772D032190A0FE82363C61000F1DF_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		bool L_2;
		L_2 = AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m72E1F5D2CE435E45651E8B970A73B9F5E3E03815(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJavaObject_Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_mE43BB2CCE4BEBFBF19C2E7C22F4E07A52002E80C_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		Il2CppChar L_2;
		L_2 = AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m22710813CDD982DC5F88C2A4067FBFE1D57A3065(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJavaObject_Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m980F8BA95C5B796FDACBDF865A7BAE431DE77362_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		double L_2;
		L_2 = AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m946CEC22A94586DE93BE0725E5F25E04589F69DE(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJavaObject_Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mE0567B52AC481152BD742506C5AB912185B2C345_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		int16_t L_2;
		L_2 = AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m08A0CE3C4BDDA756095A0A816C659587814F649E(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDC5FD095AFC55DFE596907E5B055B5774DA5B5AC_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		int32_t L_2;
		L_2 = AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1BFF486C91846170C07AAF27EF84971FF0596B90(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJavaObject_Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m63E24F733CDC205FED0CF4659E49DB4AE06ADBB9_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		int64_t L_2;
		L_2 = AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mC6DBECDD44973120E0A56068ABE886D9A7016F5C(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJavaObject_Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m517389999DF08FDB831BB7337DE4FA8D8158AF7B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		int8_t L_2;
		L_2 = AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mAE2F0D59E0CE54D36BB86C53218704FA72DC0350(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject_Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mC6CF02CBA2C4A23EF8CD0BF612F5759B8C26DFF1_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		float L_2;
		L_2 = AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m71549B5031FEE0FBB78A9CE02EDADD14DBFFB56D(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject_Call_TisIl2CppSharedGenericObject_m8DEE2A0D06B4978C5E5000815E66E6484606D138_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		Il2CppSharedGenericObject* L_2;
		L_2 = AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m39A1E2B88CD5364F83417EFB65F1C296656D2F62(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63056
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Call_TisIl2CppFullySharedGenericAny_mC798093D4EE507666B4D04C1237E82DFBBACE706_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t7ECE3320BEFB3505409302EC3AA6B828AE7E1DB5 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_ReturnType_t7ECE3320BEFB3505409302EC3AA6B828AE7E1DB5);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_methodID;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		InvokerActionInvoker3< intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, L_1, (Il2CppFullySharedGenericAny*)L_2);
		il2cpp_codegen_memcpy(il2cppRetVal, L_2, SizeOf_ReturnType_t7ECE3320BEFB3505409302EC3AA6B828AE7E1DB5);
		return;
	}
}
// Method Definition Index: 63055
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Call_TisIl2CppFullySharedGenericAny_mCA7EED8FFBB862858FF426BD7D6B191F9C24234B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t7C9CEFF53F7F785E3B0A2AA52BF0599DB9E4C7A7 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_ReturnType_t7C9CEFF53F7F785E3B0A2AA52BF0599DB9E4C7A7);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		InvokerActionInvoker3< String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, L_1, (Il2CppFullySharedGenericAny*)L_2);
		il2cpp_codegen_memcpy(il2cppRetVal, L_2, SizeOf_ReturnType_t7C9CEFF53F7F785E3B0A2AA52BF0599DB9E4C7A7);
		return;
	}
}
// Method Definition Index: 63035
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Call_TisIl2CppFullySharedGenericAny_mDA393DFB3EA03B230F3982EC0F62EED04C1B755E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_methodID;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		AndroidJavaObject__Call_m2126160FB635069207535BD0E700C3605FDB3308(__this, L_0, L_2, NULL);
		return;
	}
}
// Method Definition Index: 63034
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Call_TisIl2CppFullySharedGenericAny_mF7CDE80510F82B3C335324838828B788E1F0FAD4_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		AndroidJavaObject__Call_m4C4D7D7287030773175BDF47681EA018DFA4DF1A(__this, L_0, L_2, NULL);
		return;
	}
}
// Method Definition Index: 63054
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Call_TisIl2CppFullySharedGenericAny_TisIl2CppFullySharedGenericAny_mE94FE031A180D9514AA8E28338270133654A7412_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_ReturnType_tE77199CFC11B9237A747C8E23BCDA97324CD4769 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_4 = alloca(SizeOf_ReturnType_tE77199CFC11B9237A747C8E23BCDA97324CD4769);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_methodID;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		InvokerActionInvoker3< intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, L_2, (Il2CppFullySharedGenericAny*)L_4);
		il2cpp_codegen_memcpy(il2cppRetVal, L_4, SizeOf_ReturnType_tE77199CFC11B9237A747C8E23BCDA97324CD4769);
		return;
	}
}
// Method Definition Index: 63053
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Call_TisIl2CppFullySharedGenericAny_TisIl2CppFullySharedGenericAny_m584BD08D664D1371FF995E37EDD37FD6AA861406_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_ReturnType_t6A981EFC55AACA8DB2914A6B9C24AF2C1F0D86E3 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_4 = alloca(SizeOf_ReturnType_t6A981EFC55AACA8DB2914A6B9C24AF2C1F0D86E3);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		InvokerActionInvoker3< String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, L_2, (Il2CppFullySharedGenericAny*)L_4);
		il2cpp_codegen_memcpy(il2cppRetVal, L_4, SizeOf_ReturnType_t6A981EFC55AACA8DB2914A6B9C24AF2C1F0D86E3);
		return;
	}
}
// Method Definition Index: 63059
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject_CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mE956BC9A30BEC746DE593C53C1B8DB6A685185A6_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		bool L_2;
		L_2 = AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m41529A6A47CC3B57286AC8F9C6388E54918DF450(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63059
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m6619B03C8DA4F5A66785845A2E5B39DAEF36642A_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		int32_t L_2;
		L_2 = AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEE64E87A5A2BC0F38910643A9B91032AF38D882C(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63059
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject_CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6BB0A664B4D565DD41578EB08B9A36C16EAE9FD6_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		float L_2;
		L_2 = AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mB41AF2746C52AC8CB15EFFA72BF98A34742B7690(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63059
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject_CallStatic_TisIl2CppSharedGenericObject_mF6EA4A6FC668EA46C5309B3E3A5ABB83600F50F6_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		Il2CppSharedGenericObject* L_2;
		L_2 = AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_m8E1513A9554AAA1822CA51D0A816DCF16775AA37(__this, L_0, L_1, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_2;
	}
}
// Method Definition Index: 63060
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_CallStatic_TisIl2CppFullySharedGenericAny_m98714A38A15645388BE9EE530213C7EDB49C34AF_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t955BFF9AB851C80203957B433FC00E2046696241 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_ReturnType_t955BFF9AB851C80203957B433FC00E2046696241);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_methodID;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		InvokerActionInvoker3< intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, L_1, (Il2CppFullySharedGenericAny*)L_2);
		il2cpp_codegen_memcpy(il2cppRetVal, L_2, SizeOf_ReturnType_t955BFF9AB851C80203957B433FC00E2046696241);
		return;
	}
}
// Method Definition Index: 63059
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_CallStatic_TisIl2CppFullySharedGenericAny_m7843D14A0537A1190C739261EF49C05B1FF2ED90_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t94E13999E45FF70AA5DA5E427955FC4E439412B2 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_ReturnType_t94E13999E45FF70AA5DA5E427955FC4E439412B2);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		InvokerActionInvoker3< String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, L_1, (Il2CppFullySharedGenericAny*)L_2);
		il2cpp_codegen_memcpy(il2cppRetVal, L_2, SizeOf_ReturnType_t94E13999E45FF70AA5DA5E427955FC4E439412B2);
		return;
	}
}
// Method Definition Index: 63039
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_CallStatic_TisIl2CppFullySharedGenericAny_m9E915D63BD1F4FB05B49296EEFE25B86C30EA692_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_methodID;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		AndroidJavaObject__CallStatic_mE917E474DB9801610FB7ABE5BE749DF84CEFD48A(__this, L_0, L_2, NULL);
		return;
	}
}
// Method Definition Index: 63038
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_CallStatic_TisIl2CppFullySharedGenericAny_mB35C0A4F3C829AF99001B27BC0E99CD64736EE1A_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		AndroidJavaObject__CallStatic_mD63902D30CD5626DAEAD1D6484AF7A9ACA85590E(__this, L_0, L_2, NULL);
		return;
	}
}
// Method Definition Index: 63058
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_CallStatic_TisIl2CppFullySharedGenericAny_TisIl2CppFullySharedGenericAny_m4C097E550C3D8A103BE65545A8CB9E688DCE341E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_ReturnType_t3F35C7C5E6E91ECEB1E553CA45292DBBEC6E5501 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_4 = alloca(SizeOf_ReturnType_t3F35C7C5E6E91ECEB1E553CA45292DBBEC6E5501);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_methodID;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		InvokerActionInvoker3< intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, L_2, (Il2CppFullySharedGenericAny*)L_4);
		il2cpp_codegen_memcpy(il2cppRetVal, L_4, SizeOf_ReturnType_t3F35C7C5E6E91ECEB1E553CA45292DBBEC6E5501);
		return;
	}
}
// Method Definition Index: 63057
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_CallStatic_TisIl2CppFullySharedGenericAny_TisIl2CppFullySharedGenericAny_m9E8D43F22F21BB581F9478C8CF9B3A8059B3B8CD_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_ReturnType_t41223B870DEBD9DC66C7F3F6FDDF2CF6D061E4EB = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_4 = alloca(SizeOf_ReturnType_t41223B870DEBD9DC66C7F3F6FDDF2CF6D061E4EB);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = (ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*)SZArrayNew(ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918_il2cpp_TypeInfo_var, (uint32_t)1);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = L_1;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___1_args;
		NullCheck(L_2);
		ArrayElementTypeCheck (L_2, L_3);
		(L_2)->SetAt(static_cast<il2cpp_array_size_t>(0), (RuntimeObject*)L_3);
		InvokerActionInvoker3< String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, L_2, (Il2CppFullySharedGenericAny*)L_4);
		il2cpp_codegen_memcpy(il2cppRetVal, L_4, SizeOf_ReturnType_t41223B870DEBD9DC66C7F3F6FDDF2CF6D061E4EB);
		return;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject_FromJavaArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mA1CB7EC9473B0674763C89AC566EC664159108E9_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	bool V_0 = false;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(bool));
		bool L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		bool L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m0207DDD0AB7EE1136083B85289D4F16FD9A64ECC(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJavaObject_FromJavaArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m18459EC016E866EF307EC4D9442CD86FD9BC28B4_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	Il2CppChar V_0 = 0x0;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Il2CppChar));
		Il2CppChar L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		Il2CppChar L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m51DC292506277213F541434A51F81430E11E7226(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJavaObject_FromJavaArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m3CAD187DEBCBD88D94604015E86A37B8944847BC_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	double V_0 = 0.0;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(double));
		double L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		double L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_mF6AF89E7365D44A201C5A2C86C49EF49E2D6ADAD(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJavaObject_FromJavaArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m62441E1DA5798B8F585F5EF18386C4FB739BFFC8_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int16_t V_0 = 0;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int16_t));
		int16_t L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		int16_t L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mF6FCB78A4CF7C2FB0375E88C31C70458F55F422C(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_FromJavaArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mC1390BDE8BF42FAA78215DFE076A25F7AEA674D7_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int32_t V_0 = 0;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int32_t));
		int32_t L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		int32_t L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m02707C23958FF03FA0345C2A17F66EE379A1468C(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJavaObject_FromJavaArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m6A8E0C87ADD6FC1CA3F936C0CC0AC4AB379A8422_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int64_t V_0 = 0;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int64_t));
		int64_t L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		int64_t L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m0CADE835010213386C1C07E63F70E55498142C20(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJavaObject_FromJavaArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mB474FE99696A6DEB269E0F1C3C7DA32CD8854410_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int8_t V_0 = 0x0;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int8_t));
		int8_t L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		int8_t L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_m2BD2E924FBD117B44A2D5E741144FD7B1FCE918E(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject_FromJavaArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mD5A794B10305822230E288BE2D72329A6EE0C8A4_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	float V_0 = 0.0f;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(float));
		float L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		float L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m93BCF3156F5C823F8C2B005523A618966B0A6DBB(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject_FromJavaArray_TisIl2CppSharedGenericObject_m22D9898503E929D77563392822CD11A056613AFF_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	Il2CppSharedGenericObject* V_0 = NULL;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Il2CppSharedGenericObject*));
		Il2CppSharedGenericObject* L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		Il2CppSharedGenericObject* L_4;
		L_4 = AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_m6F1CD10E4BE61A7211FB87DA1D7D92C7FE85F8E4(L_3, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_4;
	}
}
// Method Definition Index: 63087
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_FromJavaArray_TisIl2CppFullySharedGenericAny_m4D8E7ACD1BBB7E3FEC7BA69AF6C620E70D8BBC3E_gshared (intptr_t ___0_jobject, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
	const Il2CppFullySharedGenericAny L_4 = L_2;
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
	//<source_info:<no-source>:1>
	Il2CppFullySharedGenericAny V_0 = alloca(SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
	memset(V_0, 0, SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_0, SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
		il2cpp_codegen_memcpy(L_2, V_0, SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
		il2cpp_codegen_memcpy(il2cppRetVal, L_2, SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
		return;
	}

IL_0017:
	{
		intptr_t L_3 = ___0_jobject;
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), NULL, L_3, (Il2CppFullySharedGenericAny*)L_4);
		il2cpp_codegen_box_unbox(L_4, L_5, SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC, il2cpp_rgctx_data(method->rgctx_data, 0), il2cpp_rgctx_data(method->rgctx_data, 0));
		il2cpp_codegen_memcpy(il2cppRetVal, L_5, SizeOf_ReturnType_t2602EED81B2B6E4946933FD7A1734F95B3DD02CC);
		return;
	}
}
// Method Definition Index: 63086
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m61DD76EF7E812BC3ED41BBA0D0DA1B430EC61256_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	int32_t V_0 = 0;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(int32_t));
		int32_t L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_002b:
			{
				intptr_t L_3 = ___0_jobject;
				AndroidJNISafe_DeleteLocalRef_m20303564C88A1B90E3D8D7A7D893392E18967094(L_3, NULL);
				return;
			}
		});
		try
		{
			intptr_t L_4 = ___0_jobject;
			int32_t L_5;
			L_5 = AndroidJNIHelper_ConvertFromJNIArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m02707C23958FF03FA0345C2A17F66EE379A1468C(L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
			V_0 = L_5;
			goto IL_0032;
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0032:
	{
		int32_t L_6 = V_0;
		return L_6;
	}
}
// Method Definition Index: 63086
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6191F97DB89FB7E398F8A3DF9E448A9DCD5F07EA_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	float V_0 = 0.0f;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(float));
		float L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_002b:
			{
				intptr_t L_3 = ___0_jobject;
				AndroidJNISafe_DeleteLocalRef_m20303564C88A1B90E3D8D7A7D893392E18967094(L_3, NULL);
				return;
			}
		});
		try
		{
			intptr_t L_4 = ___0_jobject;
			float L_5;
			L_5 = AndroidJNIHelper_ConvertFromJNIArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m93BCF3156F5C823F8C2B005523A618966B0A6DBB(L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
			V_0 = L_5;
			goto IL_0032;
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0032:
	{
		float L_6 = V_0;
		return L_6;
	}
}
// Method Definition Index: 63086
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisIl2CppSharedGenericObject_mB1CEFE624E92B82D234C1982AA2A7DF6AC5FD284_gshared (intptr_t ___0_jobject, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	Il2CppSharedGenericObject* V_0 = NULL;
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((&V_0), sizeof(Il2CppSharedGenericObject*));
		Il2CppSharedGenericObject* L_2 = V_0;
		return L_2;
	}

IL_0017:
	{
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_002b:
			{
				intptr_t L_3 = ___0_jobject;
				AndroidJNISafe_DeleteLocalRef_m20303564C88A1B90E3D8D7A7D893392E18967094(L_3, NULL);
				return;
			}
		});
		try
		{
			intptr_t L_4 = ___0_jobject;
			Il2CppSharedGenericObject* L_5;
			L_5 = AndroidJNIHelper_ConvertFromJNIArray_TisIl2CppSharedGenericObject_m6F1CD10E4BE61A7211FB87DA1D7D92C7FE85F8E4(L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
			V_0 = L_5;
			goto IL_0032;
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0032:
	{
		Il2CppSharedGenericObject* L_6 = V_0;
		return L_6;
	}
}
// Method Definition Index: 63086
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisIl2CppFullySharedGenericAny_m5BC2B7B267EBFB3D33E1D15F5F5DFEF99AB48E84_gshared (intptr_t ___0_jobject, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_2 = alloca(SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
	const Il2CppFullySharedGenericAny L_5 = L_2;
	const Il2CppFullySharedGenericAny L_7 = L_2;
	const Il2CppFullySharedGenericAny L_6 = alloca(SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
	//<source_info:<no-source>:1>
	Il2CppFullySharedGenericAny V_0 = alloca(SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
	memset(V_0, 0, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
	{
		intptr_t L_0 = ___0_jobject;
		bool L_1;
		L_1 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_0, 0, NULL);
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_0, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
		il2cpp_codegen_memcpy(L_2, V_0, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
		il2cpp_codegen_memcpy(il2cppRetVal, L_2, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
		return;
	}

IL_0017:
	{
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_002b:
			{
				intptr_t L_3 = ___0_jobject;
				AndroidJNISafe_DeleteLocalRef_m20303564C88A1B90E3D8D7A7D893392E18967094(L_3, NULL);
				return;
			}
		});
		try
		{
			intptr_t L_4 = ___0_jobject;
			InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), NULL, L_4, (Il2CppFullySharedGenericAny*)L_5);
			il2cpp_codegen_box_unbox(L_5, L_6, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9, il2cpp_rgctx_data(method->rgctx_data, 0), il2cpp_rgctx_data(method->rgctx_data, 0));
			il2cpp_codegen_memcpy(V_0, L_6, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
			goto IL_0032;
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0032:
	{
		il2cpp_codegen_memcpy(L_7, V_0, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
		il2cpp_codegen_memcpy(il2cppRetVal, L_7, SizeOf_ReturnType_t7F4379E8D6E3B3545F3D77660B8E3F6DA1DC4DB9);
		return;
	}
}
// Method Definition Index: 63042
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1C9D9590B9F7212AAD6D9BFFF3F2762BD090BCAB_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		int32_t L_1;
		L_1 = AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDED207A2CABEE9D78BEC022F1D4338A5091DCC50(__this, L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 63042
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject_Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mDA2BCB73A1D265234267CD05B1602089DA982C68_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		float L_1;
		L_1 = AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m599A48B8B43BD171290DD40A653B639B6F856D42(__this, L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 63043
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Get_TisIl2CppFullySharedGenericAny_m0921C141B8FD194E4F423401D827FCB547354135_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_t3D88A0B7A2BB1C07E18A9AA7AB32582B0FB37337 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_t3D88A0B7A2BB1C07E18A9AA7AB32582B0FB37337);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_fieldID;
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, (Il2CppFullySharedGenericAny*)L_1);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_FieldType_t3D88A0B7A2BB1C07E18A9AA7AB32582B0FB37337);
		return;
	}
}
// Method Definition Index: 63042
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Get_TisIl2CppFullySharedGenericAny_m390E9396B06E7DB37AB22F9F94EF31B456F01185_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_tE541E61DC1EE486A3DDC10DCA2A2DD9A2A3BADE8 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_tE541E61DC1EE486A3DDC10DCA2A2DD9A2A3BADE8);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		InvokerActionInvoker2< String_t*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, (Il2CppFullySharedGenericAny*)L_1);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_FieldType_tE541E61DC1EE486A3DDC10DCA2A2DD9A2A3BADE8);
		return;
	}
}
// Method Definition Index: 63046
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject_GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m740F3401DEA4A75BADD753EFF71D2328B4147BFC_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		int32_t L_1;
		L_1 = AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4289B7D740963C5007AD93A07FF6A0A01857DD28(__this, L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 63046
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject_GetStatic_TisIl2CppSharedGenericObject_mB51F8378DA3B0F5FFE061D86A20FA87349F98740_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		Il2CppSharedGenericObject* L_1;
		L_1 = AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_m177F1A057E8F5EF05D9158BA272E890C6210044B(__this, L_0, il2cpp_rgctx_method(method->rgctx_data, 0));
		return L_1;
	}
}
// Method Definition Index: 63047
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_GetStatic_TisIl2CppFullySharedGenericAny_m7E7CB28FD389C5D15A289C22C10EEBB9E64567B9_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_t4E8B4431DA3B0620A6961D0830B049CE0FA25BB3 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_t4E8B4431DA3B0620A6961D0830B049CE0FA25BB3);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_fieldID;
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, (Il2CppFullySharedGenericAny*)L_1);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_FieldType_t4E8B4431DA3B0620A6961D0830B049CE0FA25BB3);
		return;
	}
}
// Method Definition Index: 63046
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_GetStatic_TisIl2CppFullySharedGenericAny_mDD5B83921F6078373B85EB4F803492A68545BA7E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_tBB418E296327456981AAF34C2BEB510AEC3C4E4D = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_tBB418E296327456981AAF34C2BEB510AEC3C4E4D);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		InvokerActionInvoker2< String_t*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)), il2cpp_rgctx_method(method->rgctx_data, 0), __this, L_0, (Il2CppFullySharedGenericAny*)L_1);
		il2cpp_codegen_memcpy(il2cppRetVal, L_1, SizeOf_FieldType_tBB418E296327456981AAF34C2BEB510AEC3C4E4D);
		return;
	}
}
// Method Definition Index: 63045
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Set_TisIl2CppFullySharedGenericAny_mEB42AA9151144DAA56A324FBF5A564C71A65D35E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_tD083F29D6E32E63E26B83C297746967559FB5289 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_tD083F29D6E32E63E26B83C297746967559FB5289);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_val : &___1_val), SizeOf_FieldType_tD083F29D6E32E63E26B83C297746967559FB5289);
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? L_1: *(void**)L_1));
		return;
	}
}
// Method Definition Index: 63044
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_Set_TisIl2CppFullySharedGenericAny_m7286B3F15DA02D0B999CE939F20412740B22945E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_t5C6F84F3CFFB0874A4DA0D1C58053A25B835551E = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_t5C6F84F3CFFB0874A4DA0D1C58053A25B835551E);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_val : &___1_val), SizeOf_FieldType_t5C6F84F3CFFB0874A4DA0D1C58053A25B835551E);
		InvokerActionInvoker2< String_t*, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? L_1: *(void**)L_1));
		return;
	}
}
// Method Definition Index: 63049
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_SetStatic_TisIl2CppFullySharedGenericAny_mFB380E32D4FBFB41D3870427D3227A38E72F3D7C_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_tD9FB37412CCB11866A06B8D683ACBBD34ED06C6B = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_tD9FB37412CCB11866A06B8D683ACBBD34ED06C6B);
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_val : &___1_val), SizeOf_FieldType_tD9FB37412CCB11866A06B8D683ACBBD34ED06C6B);
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? L_1: *(void**)L_1));
		return;
	}
}
// Method Definition Index: 63048
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject_SetStatic_TisIl2CppFullySharedGenericAny_mF354DBF45CBF7D251BCCAB27BAB86047439CC292_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_t6076CC06F19BCC0301AE8C734F26CA429D2DB469 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 0));
	const Il2CppFullySharedGenericAny L_1 = alloca(SizeOf_FieldType_t6076CC06F19BCC0301AE8C734F26CA429D2DB469);
	//<source_info:<no-source>:1>
	{
		String_t* L_0 = ___0_fieldName;
		il2cpp_codegen_memcpy(L_1, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? ___1_val : &___1_val), SizeOf_FieldType_t6076CC06F19BCC0301AE8C734F26CA429D2DB469);
		InvokerActionInvoker2< String_t*, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_0, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 0)) ? L_1: *(void**)L_1));
		return;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m73D49A76D8F24AD1815CF94E87F1EF4683A38EA6_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	bool V_3 = false;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	bool V_5 = false;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	bool G_B30_0 = false;
	bool G_B35_0 = false;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				bool L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(bool), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				V_3 = L_31;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_32 = __this->___m_jobject;
				intptr_t L_33;
				L_33 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_32, NULL);
				intptr_t L_34 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_35 = V_0;
				int8_t L_36;
				L_36 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_33, L_34, L_35, NULL);
				uint8_t L_37 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_36,NULL));
				uint8_t L_38 = L_37;
				bool L_39;
				il2cpp_codegen_box_unbox((&L_38), (&L_39), sizeof(bool), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_39;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_40 = __this->___m_jobject;
				intptr_t L_41;
				L_41 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_40, NULL);
				intptr_t L_42 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_43 = V_0;
				int8_t L_44;
				L_44 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_41, L_42, L_43, NULL);
				int8_t L_45 = L_44;
				bool L_46;
				il2cpp_codegen_box_unbox((&L_45), (&L_46), sizeof(bool), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_46;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jobject;
				intptr_t L_48;
				L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
				intptr_t L_49 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_50 = V_0;
				int16_t L_51;
				L_51 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_48, L_49, L_50, NULL);
				int16_t L_52 = L_51;
				bool L_53;
				il2cpp_codegen_box_unbox((&L_52), (&L_53), sizeof(bool), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_53;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jobject;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int64_t L_58;
				L_58 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_55, L_56, L_57, NULL);
				int64_t L_59 = L_58;
				bool L_60;
				il2cpp_codegen_box_unbox((&L_59), (&L_60), sizeof(bool), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_60;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jobject;
				intptr_t L_62;
				L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
				intptr_t L_63 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_64 = V_0;
				float L_65;
				L_65 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_62, L_63, L_64, NULL);
				float L_66 = L_65;
				bool L_67;
				il2cpp_codegen_box_unbox((&L_66), (&L_67), sizeof(bool), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jobject;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				bool L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(bool), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				bool L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(bool), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(bool*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(bool*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(bool));
				bool L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(bool*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(bool));
				bool L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				bool L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mA1CB7EC9473B0674763C89AC566EC664159108E9(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(bool));
				bool L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		bool L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m72E1F5D2CE435E45651E8B970A73B9F5E3E03815_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mE3914411BE312CBBA869835158E535A06CD3EEB4(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		bool L_7;
		L_7 = AndroidJavaObject__Call_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m73D49A76D8F24AD1815CF94E87F1EF4683A38EA6(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m8B3C7C06394410B77DB9953832CB9F8C29C28E89_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	Il2CppChar V_3 = 0x0;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	Il2CppChar V_5 = 0x0;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	Il2CppChar G_B30_0 = 0x0;
	Il2CppChar G_B35_0 = 0x0;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				Il2CppChar L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(Il2CppChar), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				Il2CppChar L_33;
				il2cpp_codegen_box_unbox((&L_32), (&L_33), sizeof(Il2CppChar), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_33;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jobject;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				Il2CppChar L_41;
				il2cpp_codegen_box_unbox((&L_40), (&L_41), sizeof(Il2CppChar), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_41;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jobject;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				Il2CppChar L_48;
				il2cpp_codegen_box_unbox((&L_47), (&L_48), sizeof(Il2CppChar), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_48;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jobject;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_50, L_51, L_52, NULL);
				int16_t L_54 = L_53;
				Il2CppChar L_55;
				il2cpp_codegen_box_unbox((&L_54), (&L_55), sizeof(Il2CppChar), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_55;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_56 = __this->___m_jobject;
				intptr_t L_57;
				L_57 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_56, NULL);
				intptr_t L_58 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_59 = V_0;
				int64_t L_60;
				L_60 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_57, L_58, L_59, NULL);
				int64_t L_61 = L_60;
				Il2CppChar L_62;
				il2cpp_codegen_box_unbox((&L_61), (&L_62), sizeof(Il2CppChar), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_62;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_63 = __this->___m_jobject;
				intptr_t L_64;
				L_64 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_63, NULL);
				intptr_t L_65 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_66 = V_0;
				float L_67;
				L_67 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_64, L_65, L_66, NULL);
				float L_68 = L_67;
				Il2CppChar L_69;
				il2cpp_codegen_box_unbox((&L_68), (&L_69), sizeof(Il2CppChar), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_69;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_70 = __this->___m_jobject;
				intptr_t L_71;
				L_71 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_70, NULL);
				intptr_t L_72 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_73 = V_0;
				double L_74;
				L_74 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_71, L_72, L_73, NULL);
				double L_75 = L_74;
				Il2CppChar L_76;
				il2cpp_codegen_box_unbox((&L_75), (&L_76), sizeof(Il2CppChar), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_76;
				goto IL_03ea;
			}

IL_0246_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_77 = __this->___m_jobject;
				intptr_t L_78;
				L_78 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_77, NULL);
				intptr_t L_79 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_80 = V_0;
				Il2CppChar L_81;
				L_81 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_78, L_79, L_80, NULL);
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(Il2CppChar*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(Il2CppChar*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppChar));
				Il2CppChar L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(Il2CppChar*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppChar));
				Il2CppChar L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				Il2CppChar L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m18459EC016E866EF307EC4D9442CD86FD9BC28B4(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppChar));
				Il2CppChar L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		Il2CppChar L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppChar AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m22710813CDD982DC5F88C2A4067FBFE1D57A3065_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m791647AA6EAD3B88467418F51CF53F5755AB38BA(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		Il2CppChar L_7;
		L_7 = AndroidJavaObject__Call_TisChar_t521A6F19B456D956AF452D926C32709DC03D6B17_m8B3C7C06394410B77DB9953832CB9F8C29C28E89(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m0BA90472306DF5C3A1491FC3EB052DD58FA468C3_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	double V_3 = 0.0;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	double V_5 = 0.0;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	double G_B30_0 = 0.0;
	double G_B35_0 = 0.0;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				double L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(double), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				double L_33;
				il2cpp_codegen_box_unbox((&L_32), (&L_33), sizeof(double), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_33;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jobject;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				double L_41;
				il2cpp_codegen_box_unbox((&L_40), (&L_41), sizeof(double), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_41;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jobject;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				double L_48;
				il2cpp_codegen_box_unbox((&L_47), (&L_48), sizeof(double), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_48;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jobject;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_50, L_51, L_52, NULL);
				int16_t L_54 = L_53;
				double L_55;
				il2cpp_codegen_box_unbox((&L_54), (&L_55), sizeof(double), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_55;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_56 = __this->___m_jobject;
				intptr_t L_57;
				L_57 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_56, NULL);
				intptr_t L_58 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_59 = V_0;
				int64_t L_60;
				L_60 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_57, L_58, L_59, NULL);
				int64_t L_61 = L_60;
				double L_62;
				il2cpp_codegen_box_unbox((&L_61), (&L_62), sizeof(double), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_62;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_63 = __this->___m_jobject;
				intptr_t L_64;
				L_64 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_63, NULL);
				intptr_t L_65 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_66 = V_0;
				float L_67;
				L_67 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_64, L_65, L_66, NULL);
				float L_68 = L_67;
				double L_69;
				il2cpp_codegen_box_unbox((&L_68), (&L_69), sizeof(double), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_69;
				goto IL_03ea;
			}

IL_0209_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_70 = __this->___m_jobject;
				intptr_t L_71;
				L_71 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_70, NULL);
				intptr_t L_72 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_73 = V_0;
				double L_74;
				L_74 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_71, L_72, L_73, NULL);
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				double L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(double), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(double*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(double*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(double));
				double L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(double*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(double));
				double L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				double L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m3CAD187DEBCBD88D94604015E86A37B8944847BC(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(double));
				double L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		double L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR double AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m946CEC22A94586DE93BE0725E5F25E04589F69DE_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m44C60AAE64EE97BFD5D9D302E7BE5BE20E6862B3(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		double L_7;
		L_7 = AndroidJavaObject__Call_TisDouble_tE150EF3D1D43DEE85D533810AB4C742307EEDE5F_m0BA90472306DF5C3A1491FC3EB052DD58FA468C3(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mA616697346ABF4A696B8F63B5F85AA92777EA400_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	int16_t V_3 = 0;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	int16_t V_5 = 0;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	int16_t G_B30_0 = 0;
	int16_t G_B35_0 = 0;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				int16_t L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(int16_t), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				int16_t L_33;
				il2cpp_codegen_box_unbox((&L_32), (&L_33), sizeof(int16_t), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_33;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jobject;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				int16_t L_41;
				il2cpp_codegen_box_unbox((&L_40), (&L_41), sizeof(int16_t), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_41;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jobject;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				int16_t L_48;
				il2cpp_codegen_box_unbox((&L_47), (&L_48), sizeof(int16_t), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_48;
				goto IL_03ea;
			}

IL_0152_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jobject;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_50, L_51, L_52, NULL);
				V_3 = L_53;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jobject;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int64_t L_58;
				L_58 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_55, L_56, L_57, NULL);
				int64_t L_59 = L_58;
				int16_t L_60;
				il2cpp_codegen_box_unbox((&L_59), (&L_60), sizeof(int16_t), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_60;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jobject;
				intptr_t L_62;
				L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
				intptr_t L_63 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_64 = V_0;
				float L_65;
				L_65 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_62, L_63, L_64, NULL);
				float L_66 = L_65;
				int16_t L_67;
				il2cpp_codegen_box_unbox((&L_66), (&L_67), sizeof(int16_t), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jobject;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				int16_t L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(int16_t), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				int16_t L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(int16_t), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(int16_t*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(int16_t*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int16_t));
				int16_t L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(int16_t*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int16_t));
				int16_t L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				int16_t L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m62441E1DA5798B8F585F5EF18386C4FB739BFFC8(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int16_t));
				int16_t L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		int16_t L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int16_t AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m08A0CE3C4BDDA756095A0A816C659587814F649E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_m3B57915B048BD436E00741CD146D62AC775E7BA3(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		int16_t L_7;
		L_7 = AndroidJavaObject__Call_TisInt16_tB8EF286A9C33492FA6E6D6E67320BE93E794A175_mA616697346ABF4A696B8F63B5F85AA92777EA400(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m0D60BAB951F2C313123D62CAB81EEA263F2F0750_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	int32_t V_5 = 0;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	int32_t G_B30_0 = 0;
	int32_t G_B35_0 = 0;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				V_3 = L_24;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_25 = __this->___m_jobject;
				intptr_t L_26;
				L_26 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_25, NULL);
				intptr_t L_27 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_28 = V_0;
				bool L_29;
				L_29 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_26, L_27, L_28, NULL);
				bool L_30 = L_29;
				int32_t L_31;
				il2cpp_codegen_box_unbox((&L_30), (&L_31), sizeof(int32_t), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_31;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_32 = __this->___m_jobject;
				intptr_t L_33;
				L_33 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_32, NULL);
				intptr_t L_34 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_35 = V_0;
				int8_t L_36;
				L_36 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_33, L_34, L_35, NULL);
				uint8_t L_37 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_36,NULL));
				uint8_t L_38 = L_37;
				int32_t L_39;
				il2cpp_codegen_box_unbox((&L_38), (&L_39), sizeof(int32_t), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_39;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_40 = __this->___m_jobject;
				intptr_t L_41;
				L_41 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_40, NULL);
				intptr_t L_42 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_43 = V_0;
				int8_t L_44;
				L_44 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_41, L_42, L_43, NULL);
				int8_t L_45 = L_44;
				int32_t L_46;
				il2cpp_codegen_box_unbox((&L_45), (&L_46), sizeof(int32_t), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_46;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jobject;
				intptr_t L_48;
				L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
				intptr_t L_49 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_50 = V_0;
				int16_t L_51;
				L_51 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_48, L_49, L_50, NULL);
				int16_t L_52 = L_51;
				int32_t L_53;
				il2cpp_codegen_box_unbox((&L_52), (&L_53), sizeof(int32_t), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_53;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jobject;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int64_t L_58;
				L_58 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_55, L_56, L_57, NULL);
				int64_t L_59 = L_58;
				int32_t L_60;
				il2cpp_codegen_box_unbox((&L_59), (&L_60), sizeof(int32_t), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_60;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jobject;
				intptr_t L_62;
				L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
				intptr_t L_63 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_64 = V_0;
				float L_65;
				L_65 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_62, L_63, L_64, NULL);
				float L_66 = L_65;
				int32_t L_67;
				il2cpp_codegen_box_unbox((&L_66), (&L_67), sizeof(int32_t), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jobject;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				int32_t L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(int32_t), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				int32_t L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(int32_t), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(int32_t*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(int32_t*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int32_t));
				int32_t L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(int32_t*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int32_t));
				int32_t L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				int32_t L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mC1390BDE8BF42FAA78215DFE076A25F7AEA674D7(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int32_t));
				int32_t L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		int32_t L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1BFF486C91846170C07AAF27EF84971FF0596B90_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mF87CC238DDBB3C90C35846E626548847CC7E319D(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		int32_t L_7;
		L_7 = AndroidJavaObject__Call_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m0D60BAB951F2C313123D62CAB81EEA263F2F0750(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4B925872A7B673BC9981AB72E3A229BBE0E1119E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	int64_t V_3 = 0;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	int64_t V_5 = 0;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	int64_t G_B30_0 = 0;
	int64_t G_B35_0 = 0;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				int64_t L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(int64_t), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				int64_t L_33;
				il2cpp_codegen_box_unbox((&L_32), (&L_33), sizeof(int64_t), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_33;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jobject;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				int64_t L_41;
				il2cpp_codegen_box_unbox((&L_40), (&L_41), sizeof(int64_t), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_41;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jobject;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				int64_t L_48;
				il2cpp_codegen_box_unbox((&L_47), (&L_48), sizeof(int64_t), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_48;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jobject;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_50, L_51, L_52, NULL);
				int16_t L_54 = L_53;
				int64_t L_55;
				il2cpp_codegen_box_unbox((&L_54), (&L_55), sizeof(int64_t), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_55;
				goto IL_03ea;
			}

IL_018f_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_56 = __this->___m_jobject;
				intptr_t L_57;
				L_57 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_56, NULL);
				intptr_t L_58 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_59 = V_0;
				int64_t L_60;
				L_60 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_57, L_58, L_59, NULL);
				V_3 = L_60;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jobject;
				intptr_t L_62;
				L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
				intptr_t L_63 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_64 = V_0;
				float L_65;
				L_65 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_62, L_63, L_64, NULL);
				float L_66 = L_65;
				int64_t L_67;
				il2cpp_codegen_box_unbox((&L_66), (&L_67), sizeof(int64_t), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jobject;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				int64_t L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(int64_t), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				int64_t L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(int64_t), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(int64_t*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(int64_t*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int64_t));
				int64_t L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(int64_t*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int64_t));
				int64_t L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				int64_t L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m6A8E0C87ADD6FC1CA3F936C0CC0AC4AB379A8422(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int64_t));
				int64_t L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		int64_t L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int64_t AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_mC6DBECDD44973120E0A56068ABE886D9A7016F5C_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m9665934CC08FD80EB16882C4EA6FDBC5D9C3A175(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		int64_t L_7;
		L_7 = AndroidJavaObject__Call_TisInt64_t092CFB123BE63C28ACDAF65C68F21A526050DBA3_m4B925872A7B673BC9981AB72E3A229BBE0E1119E(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mA3300F1D8CC0B110426EAE23F5FB7F27FD8AE76B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	int8_t V_3 = 0x0;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	int8_t V_5 = 0x0;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	int8_t G_B30_0 = 0x0;
	int8_t G_B35_0 = 0x0;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				int8_t L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(int8_t), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				int8_t L_33;
				il2cpp_codegen_box_unbox((&L_32), (&L_33), sizeof(int8_t), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_33;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jobject;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				int8_t L_41;
				il2cpp_codegen_box_unbox((&L_40), (&L_41), sizeof(int8_t), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_41;
				goto IL_03ea;
			}

IL_0115_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jobject;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_43, L_44, L_45, NULL);
				V_3 = L_46;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jobject;
				intptr_t L_48;
				L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
				intptr_t L_49 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_50 = V_0;
				int16_t L_51;
				L_51 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_48, L_49, L_50, NULL);
				int16_t L_52 = L_51;
				int8_t L_53;
				il2cpp_codegen_box_unbox((&L_52), (&L_53), sizeof(int8_t), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_53;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jobject;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int64_t L_58;
				L_58 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_55, L_56, L_57, NULL);
				int64_t L_59 = L_58;
				int8_t L_60;
				il2cpp_codegen_box_unbox((&L_59), (&L_60), sizeof(int8_t), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_60;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jobject;
				intptr_t L_62;
				L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
				intptr_t L_63 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_64 = V_0;
				float L_65;
				L_65 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_62, L_63, L_64, NULL);
				float L_66 = L_65;
				int8_t L_67;
				il2cpp_codegen_box_unbox((&L_66), (&L_67), sizeof(int8_t), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jobject;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				int8_t L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(int8_t), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				int8_t L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(int8_t), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(int8_t*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(int8_t*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int8_t));
				int8_t L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(int8_t*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int8_t));
				int8_t L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				int8_t L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mB474FE99696A6DEB269E0F1C3C7DA32CD8854410(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int8_t));
				int8_t L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		int8_t L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int8_t AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mAE2F0D59E0CE54D36BB86C53218704FA72DC0350_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mEF6ACD1C6C5237E0446DB9DBDAC40BA2660849BB(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		int8_t L_7;
		L_7 = AndroidJavaObject__Call_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mA3300F1D8CC0B110426EAE23F5FB7F27FD8AE76B(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8EDEAF89FE2C5B65277B0FC57FAD4FA9253E1140_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	float V_3 = 0.0f;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	float V_5 = 0.0f;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	float G_B30_0 = 0.0f;
	float G_B35_0 = 0.0f;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				float L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(float), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				float L_33;
				il2cpp_codegen_box_unbox((&L_32), (&L_33), sizeof(float), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_33;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jobject;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				float L_41;
				il2cpp_codegen_box_unbox((&L_40), (&L_41), sizeof(float), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_41;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jobject;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				float L_48;
				il2cpp_codegen_box_unbox((&L_47), (&L_48), sizeof(float), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_48;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jobject;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_50, L_51, L_52, NULL);
				int16_t L_54 = L_53;
				float L_55;
				il2cpp_codegen_box_unbox((&L_54), (&L_55), sizeof(float), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_55;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_56 = __this->___m_jobject;
				intptr_t L_57;
				L_57 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_56, NULL);
				intptr_t L_58 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_59 = V_0;
				int64_t L_60;
				L_60 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_57, L_58, L_59, NULL);
				int64_t L_61 = L_60;
				float L_62;
				il2cpp_codegen_box_unbox((&L_61), (&L_62), sizeof(float), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_62;
				goto IL_03ea;
			}

IL_01cc_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_63 = __this->___m_jobject;
				intptr_t L_64;
				L_64 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_63, NULL);
				intptr_t L_65 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_66 = V_0;
				float L_67;
				L_67 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_64, L_65, L_66, NULL);
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jobject;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				float L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(float), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				float L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(float), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jobject;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_83, L_84, L_85, NULL);
				V_3 = ((*(float*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(float*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(float));
				float L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jobject;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(float*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(float));
				float L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jobject;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_113, L_114, L_115, NULL);
				float L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mD5A794B10305822230E288BE2D72329A6EE0C8A4(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(float));
				float L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		float L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m71549B5031FEE0FBB78A9CE02EDADD14DBFFB56D_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mA3FFDC6948922BEFC787A297D5483BA277EEFF0C(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		float L_7;
		L_7 = AndroidJavaObject__Call_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m8EDEAF89FE2C5B65277B0FC57FAD4FA9253E1140(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m34A45129A0AA339AC2E31492EF928112EDECC197_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	Il2CppSharedGenericObject* V_3 = NULL;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	Il2CppSharedGenericObject* V_5 = NULL;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	Il2CppSharedGenericObject* G_B30_0 = NULL;
	Il2CppSharedGenericObject* G_B35_0 = NULL;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jobject;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				RuntimeObject* L_26 = Box(il2cpp_defaults.int32_class, &L_25);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_26, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jobject;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				RuntimeObject* L_33 = Box(il2cpp_defaults.boolean_class, &L_32);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_33, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jobject;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				RuntimeObject* L_41 = Box(il2cpp_defaults.byte_class, &L_40);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_41, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jobject;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				RuntimeObject* L_48 = Box(il2cpp_defaults.sbyte_class, &L_47);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_48, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jobject;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_50, L_51, L_52, NULL);
				int16_t L_54 = L_53;
				RuntimeObject* L_55 = Box(il2cpp_defaults.int16_class, &L_54);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_55, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_56 = __this->___m_jobject;
				intptr_t L_57;
				L_57 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_56, NULL);
				intptr_t L_58 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_59 = V_0;
				int64_t L_60;
				L_60 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_57, L_58, L_59, NULL);
				int64_t L_61 = L_60;
				RuntimeObject* L_62 = Box(il2cpp_defaults.int64_class, &L_61);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_62, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_63 = __this->___m_jobject;
				intptr_t L_64;
				L_64 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_63, NULL);
				intptr_t L_65 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_66 = V_0;
				float L_67;
				L_67 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_64, L_65, L_66, NULL);
				float L_68 = L_67;
				RuntimeObject* L_69 = Box(il2cpp_defaults.single_class, &L_68);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_69, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_70 = __this->___m_jobject;
				intptr_t L_71;
				L_71 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_70, NULL);
				intptr_t L_72 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_73 = V_0;
				double L_74;
				L_74 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_71, L_72, L_73, NULL);
				double L_75 = L_74;
				RuntimeObject* L_76 = Box(il2cpp_defaults.double_class, &L_75);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_76, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_77 = __this->___m_jobject;
				intptr_t L_78;
				L_78 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_77, NULL);
				intptr_t L_79 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_80 = V_0;
				Il2CppChar L_81;
				L_81 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_78, L_79, L_80, NULL);
				Il2CppChar L_82 = L_81;
				RuntimeObject* L_83 = Box(il2cpp_defaults.char_class, &L_82);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_83, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0286_1:
			{
				bool L_84 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_84)
				{
					goto IL_02be_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_85 = __this->___m_jobject;
				intptr_t L_86;
				L_86 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_85, NULL);
				intptr_t L_87 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_88 = V_0;
				String_t* L_89;
				L_89 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_86, L_87, L_88, NULL);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_89, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_02be_1:
			{
				bool L_90 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_90)
				{
					goto IL_0319_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_91 = __this->___m_jobject;
				intptr_t L_92;
				L_92 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_91, NULL);
				intptr_t L_93 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_94 = V_0;
				intptr_t L_95;
				L_95 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_92, L_93, L_94, NULL);
				V_4 = L_95;
				intptr_t L_96 = V_4;
				bool L_97;
				L_97 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_96, 0, NULL);
				if (L_97)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_98 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_99 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_99, L_98, NULL);
				G_B30_0 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_99, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppSharedGenericObject*));
				Il2CppSharedGenericObject* L_100 = V_5;
				G_B30_0 = L_100;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				bool L_101 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_101)
				{
					goto IL_0371_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_102 = __this->___m_jobject;
				intptr_t L_103;
				L_103 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_102, NULL);
				intptr_t L_104 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_105 = V_0;
				intptr_t L_106;
				L_106 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_103, L_104, L_105, NULL);
				V_6 = L_106;
				intptr_t L_107 = V_6;
				bool L_108;
				L_108 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_107, 0, NULL);
				if (L_108)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_109 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_110 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_110, L_109, NULL);
				G_B35_0 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_110, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppSharedGenericObject*));
				Il2CppSharedGenericObject* L_111 = V_5;
				G_B35_0 = L_111;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_112 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_113;
				L_113 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_112, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_114 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_115;
				L_115 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_114, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_116;
				L_116 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_113, L_115, NULL);
				if (!L_116)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_117 = __this->___m_jobject;
				intptr_t L_118;
				L_118 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_117, NULL);
				intptr_t L_119 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_120 = V_0;
				intptr_t L_121;
				L_121 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_118, L_119, L_120, NULL);
				Il2CppSharedGenericObject* L_122;
				L_122 = AndroidJavaObject_FromJavaArray_TisIl2CppSharedGenericObject_m22D9898503E929D77563392822CD11A056613AFF(L_121, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_122;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_123 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_124;
				L_124 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_123, NULL);
				Type_t* L_125 = L_124;
				if (L_125)
				{
					G_B40_0 = L_125;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_125;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_126;
				L_126 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_126;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_127;
				L_127 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_128 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_128, L_127, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_128, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppSharedGenericObject*));
				Il2CppSharedGenericObject* L_129 = V_5;
				V_3 = L_129;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		Il2CppSharedGenericObject* L_130 = V_3;
		return L_130;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m39A1E2B88CD5364F83417EFB65F1C296656D2F62_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_m95122CB55DFAF463B14CBBA51C2B0811790DB90B(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		Il2CppSharedGenericObject* L_7;
		L_7 = AndroidJavaObject__Call_TisIl2CppSharedGenericObject_m34A45129A0AA339AC2E31492EF928112EDECC197(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63071
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Call_TisIl2CppFullySharedGenericAny_m4720E88EC57849809AFC4A771495414E75120D8F_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_27 = alloca(SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	const Il2CppFullySharedGenericAny L_35 = L_27;
	const Il2CppFullySharedGenericAny L_44 = L_27;
	const Il2CppFullySharedGenericAny L_52 = L_27;
	const Il2CppFullySharedGenericAny L_60 = L_27;
	const Il2CppFullySharedGenericAny L_68 = L_27;
	const Il2CppFullySharedGenericAny L_76 = L_27;
	const Il2CppFullySharedGenericAny L_84 = L_27;
	const Il2CppFullySharedGenericAny L_92 = L_27;
	const Il2CppFullySharedGenericAny L_99 = L_27;
	const Il2CppFullySharedGenericAny L_111 = L_27;
	const Il2CppFullySharedGenericAny L_113 = L_27;
	const Il2CppFullySharedGenericAny L_124 = L_27;
	const Il2CppFullySharedGenericAny L_126 = L_27;
	const Il2CppFullySharedGenericAny L_137 = L_27;
	const Il2CppFullySharedGenericAny L_144 = L_27;
	const Il2CppFullySharedGenericAny L_145 = L_27;
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	Il2CppFullySharedGenericAny V_3 = alloca(SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	memset(V_3, 0, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	Il2CppFullySharedGenericAny V_5 = alloca(SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	memset(V_5, 0, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	Il2CppFullySharedGenericAny G_B30_0 = alloca(SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	memset(G_B30_0, 0, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	Il2CppFullySharedGenericAny G_B35_0 = alloca(SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	memset(G_B35_0, 0, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				bool L_20 = (il2cpp_defaults.int32_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_20)
				{
					goto IL_0090_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_21 = __this->___m_jobject;
				intptr_t L_22;
				L_22 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_21, NULL);
				intptr_t L_23 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_24 = V_0;
				int32_t L_25;
				L_25 = AndroidJNISafe_CallIntMethod_m60318205A7EAD0C5CC0643106A7044F1563DCC0E(L_22, L_23, L_24, NULL);
				int32_t L_26 = L_25;
				il2cpp_codegen_box_unbox((&L_26), L_27, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_27, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0090_1:
			{
				bool L_28 = (il2cpp_defaults.boolean_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_28)
				{
					goto IL_00cd_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_29 = __this->___m_jobject;
				intptr_t L_30;
				L_30 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_29, NULL);
				intptr_t L_31 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_32 = V_0;
				bool L_33;
				L_33 = AndroidJNISafe_CallBooleanMethod_m2F5824C9EA5D1586C7E555F9F8DE01D84757D972(L_30, L_31, L_32, NULL);
				bool L_34 = L_33;
				il2cpp_codegen_box_unbox((&L_34), L_35, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_35, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_00cd_1:
			{
				bool L_36 = (il2cpp_defaults.byte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_36)
				{
					goto IL_0115_1;
				}
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_37 = __this->___m_jobject;
				intptr_t L_38;
				L_38 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_37, NULL);
				intptr_t L_39 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_40 = V_0;
				int8_t L_41;
				L_41 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_38, L_39, L_40, NULL);
				uint8_t L_42 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_41,NULL));
				uint8_t L_43 = L_42;
				il2cpp_codegen_box_unbox((&L_43), L_44, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_44, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0115_1:
			{
				bool L_45 = (il2cpp_defaults.sbyte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_45)
				{
					goto IL_0152_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_46 = __this->___m_jobject;
				intptr_t L_47;
				L_47 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_46, NULL);
				intptr_t L_48 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_49 = V_0;
				int8_t L_50;
				L_50 = AndroidJNISafe_CallSByteMethod_m03F9BD1288769A14F5CE8477DACDD62F6D0B77E7(L_47, L_48, L_49, NULL);
				int8_t L_51 = L_50;
				il2cpp_codegen_box_unbox((&L_51), L_52, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_52, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0152_1:
			{
				bool L_53 = (il2cpp_defaults.int16_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_53)
				{
					goto IL_018f_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jobject;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int16_t L_58;
				L_58 = AndroidJNISafe_CallShortMethod_m7C82D811B75161D4567651B0D85E5F7A2ED83A97(L_55, L_56, L_57, NULL);
				int16_t L_59 = L_58;
				il2cpp_codegen_box_unbox((&L_59), L_60, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_60, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_018f_1:
			{
				bool L_61 = (il2cpp_defaults.int64_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_61)
				{
					goto IL_01cc_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_62 = __this->___m_jobject;
				intptr_t L_63;
				L_63 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_62, NULL);
				intptr_t L_64 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_65 = V_0;
				int64_t L_66;
				L_66 = AndroidJNISafe_CallLongMethod_mD04CC840004334A567747BD526F88A813CB833B6(L_63, L_64, L_65, NULL);
				int64_t L_67 = L_66;
				il2cpp_codegen_box_unbox((&L_67), L_68, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_68, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_01cc_1:
			{
				bool L_69 = (il2cpp_defaults.single_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_69)
				{
					goto IL_0209_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_70 = __this->___m_jobject;
				intptr_t L_71;
				L_71 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_70, NULL);
				intptr_t L_72 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_73 = V_0;
				float L_74;
				L_74 = AndroidJNISafe_CallFloatMethod_m7437E60E0985885D721F1592E4DACF8246F69BBE(L_71, L_72, L_73, NULL);
				float L_75 = L_74;
				il2cpp_codegen_box_unbox((&L_75), L_76, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_76, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0209_1:
			{
				bool L_77 = (il2cpp_defaults.double_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_77)
				{
					goto IL_0246_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_78 = __this->___m_jobject;
				intptr_t L_79;
				L_79 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_78, NULL);
				intptr_t L_80 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_81 = V_0;
				double L_82;
				L_82 = AndroidJNISafe_CallDoubleMethod_m01B318F7CA4F90C54D689CF0CD84DF312E68CB5E(L_79, L_80, L_81, NULL);
				double L_83 = L_82;
				il2cpp_codegen_box_unbox((&L_83), L_84, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_84, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0246_1:
			{
				bool L_85 = (il2cpp_defaults.char_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_85)
				{
					goto IL_03d1_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_86 = __this->___m_jobject;
				intptr_t L_87;
				L_87 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_86, NULL);
				intptr_t L_88 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_89 = V_0;
				Il2CppChar L_90;
				L_90 = AndroidJNISafe_CallCharMethod_mB777FAF5E9D1BFF480B7EDD5AA5352F30797E1DD(L_87, L_88, L_89, NULL);
				Il2CppChar L_91 = L_90;
				il2cpp_codegen_box_unbox((&L_91), L_92, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147, il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_92, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0286_1:
			{
				bool L_93 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_93)
				{
					goto IL_02be_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_94 = __this->___m_jobject;
				intptr_t L_95;
				L_95 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_94, NULL);
				intptr_t L_96 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_97 = V_0;
				String_t* L_98;
				L_98 = AndroidJNISafe_CallStringMethod_m4E40DA54A224C0C10A8C600CAC1C2C838B69264C(L_95, L_96, L_97, NULL);
				void* L_100 = UnBox_Any((RuntimeObject*)L_98, il2cpp_rgctx_data(method->rgctx_data, 1), L_99);
				il2cpp_codegen_memcpy(V_3, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_100)), SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_02be_1:
			{
				bool L_101 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_101)
				{
					goto IL_0319_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_102 = __this->___m_jobject;
				intptr_t L_103;
				L_103 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_102, NULL);
				intptr_t L_104 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_105 = V_0;
				intptr_t L_106;
				L_106 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_103, L_104, L_105, NULL);
				V_4 = L_106;
				intptr_t L_107 = V_4;
				bool L_108;
				L_108 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_107, 0, NULL);
				if (L_108)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_109 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_110 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_110, L_109, NULL);
				void* L_112 = UnBox_Any((RuntimeObject*)L_110, il2cpp_rgctx_data(method->rgctx_data, 1), L_111);
				il2cpp_codegen_memcpy(G_B30_0, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_112)), SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_5, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				il2cpp_codegen_memcpy(L_113, V_5, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				il2cpp_codegen_memcpy(G_B30_0, L_113, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
			}

IL_0313_1:
			{
				il2cpp_codegen_memcpy(V_3, G_B30_0, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0319_1:
			{
				bool L_114 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_114)
				{
					goto IL_0371_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_115 = __this->___m_jobject;
				intptr_t L_116;
				L_116 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_115, NULL);
				intptr_t L_117 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_118 = V_0;
				intptr_t L_119;
				L_119 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_116, L_117, L_118, NULL);
				V_6 = L_119;
				intptr_t L_120 = V_6;
				bool L_121;
				L_121 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_120, 0, NULL);
				if (L_121)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_122 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_123 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_123, L_122, NULL);
				void* L_125 = UnBox_Any((RuntimeObject*)L_123, il2cpp_rgctx_data(method->rgctx_data, 1), L_124);
				il2cpp_codegen_memcpy(G_B35_0, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_125)), SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_5, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				il2cpp_codegen_memcpy(L_126, V_5, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				il2cpp_codegen_memcpy(G_B35_0, L_126, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
			}

IL_036e_1:
			{
				il2cpp_codegen_memcpy(V_3, G_B35_0, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_127 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_128;
				L_128 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_127, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_129 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_130;
				L_130 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_129, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_131;
				L_131 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_128, L_130, NULL);
				if (!L_131)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_132 = __this->___m_jobject;
				intptr_t L_133;
				L_133 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_132, NULL);
				intptr_t L_134 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_135 = V_0;
				intptr_t L_136;
				L_136 = AndroidJNISafe_CallObjectMethod_m12F882542956F2920187AADCD0295D4E32124BEF(L_133, L_134, L_135, NULL);
				InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_136, (Il2CppFullySharedGenericAny*)L_137);
				il2cpp_codegen_memcpy(V_3, L_137, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_138 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_139;
				L_139 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_138, NULL);
				Type_t* L_140 = L_139;
				if (L_140)
				{
					G_B40_0 = L_140;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_140;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_141;
				L_141 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_141;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_142;
				L_142 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_143 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_143, L_142, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_143, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_5, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				il2cpp_codegen_memcpy(L_144, V_5, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				il2cpp_codegen_memcpy(V_3, L_144, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		il2cpp_codegen_memcpy(L_145, V_3, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
		il2cpp_codegen_memcpy(il2cppRetVal, L_145, SizeOf_ReturnType_t79ECD6DB4ABC339D39F04EE50C832B65A84F5147);
		return;
	}
}
// Method Definition Index: 63070
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Call_TisIl2CppFullySharedGenericAny_mA1A866611778FF72A2067F7E30B32913C4EB4904_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t0FD1385ACD92B5652F803E183304929EDB7632D9 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_7 = alloca(SizeOf_ReturnType_t0FD1385ACD92B5652F803E183304929EDB7632D9);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_1, L_2, L_3, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		InvokerActionInvoker3< intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_5, L_6, (Il2CppFullySharedGenericAny*)L_7);
		il2cpp_codegen_memcpy(il2cppRetVal, L_7, SizeOf_ReturnType_t0FD1385ACD92B5652F803E183304929EDB7632D9);
		return;
	}
}
// Method Definition Index: 63079
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m9EDC162A865064239ADA4058CD2A81B1A2CF6D0F_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	bool V_3 = false;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	bool V_5 = false;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	bool G_B30_0 = false;
	bool G_B35_0 = false;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jclass;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallStaticIntMethod_m915549FA8FD7FB93B57A9708AD759488EA64418C(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				bool L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(bool), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jclass;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallStaticBooleanMethod_m652685AC18F590965249C0F9B107C00C142595BB(L_28, L_29, L_30, NULL);
				V_3 = L_31;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_32 = __this->___m_jclass;
				intptr_t L_33;
				L_33 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_32, NULL);
				intptr_t L_34 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_35 = V_0;
				int8_t L_36;
				L_36 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_33, L_34, L_35, NULL);
				uint8_t L_37 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_36,NULL));
				uint8_t L_38 = L_37;
				bool L_39;
				il2cpp_codegen_box_unbox((&L_38), (&L_39), sizeof(bool), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_39;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_40 = __this->___m_jclass;
				intptr_t L_41;
				L_41 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_40, NULL);
				intptr_t L_42 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_43 = V_0;
				int8_t L_44;
				L_44 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_41, L_42, L_43, NULL);
				int8_t L_45 = L_44;
				bool L_46;
				il2cpp_codegen_box_unbox((&L_45), (&L_46), sizeof(bool), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_46;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jclass;
				intptr_t L_48;
				L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
				intptr_t L_49 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_50 = V_0;
				int16_t L_51;
				L_51 = AndroidJNISafe_CallStaticShortMethod_m8330383670ECCD7E24CDD68C419745E486FA6426(L_48, L_49, L_50, NULL);
				int16_t L_52 = L_51;
				bool L_53;
				il2cpp_codegen_box_unbox((&L_52), (&L_53), sizeof(bool), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_53;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jclass;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int64_t L_58;
				L_58 = AndroidJNISafe_CallStaticLongMethod_mDDE01239BEFCF007ECE05E51A249B3EB5BB61234(L_55, L_56, L_57, NULL);
				int64_t L_59 = L_58;
				bool L_60;
				il2cpp_codegen_box_unbox((&L_59), (&L_60), sizeof(bool), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_60;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jclass;
				intptr_t L_62;
				L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
				intptr_t L_63 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_64 = V_0;
				float L_65;
				L_65 = AndroidJNISafe_CallStaticFloatMethod_m3F5419A10B9DF599352938B2BAD8866F8F112364(L_62, L_63, L_64, NULL);
				float L_66 = L_65;
				bool L_67;
				il2cpp_codegen_box_unbox((&L_66), (&L_67), sizeof(bool), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jclass;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallStaticDoubleMethod_m73F1D51601D6849EE480389B4E43AED68C42B2B5(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				bool L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(bool), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jclass;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallStaticCharMethod_mC4B40190CE095728E823AB8B724ECDC8F4B36155(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				bool L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(bool), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jclass;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStaticStringMethod_m4E150E34CC6DBF27A955F8DAEE5941D6E10879C0(L_83, L_84, L_85, NULL);
				V_3 = ((*(bool*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jclass;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(bool*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(bool));
				bool L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jclass;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(bool*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(bool));
				bool L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jclass;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_113, L_114, L_115, NULL);
				bool L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mA1CB7EC9473B0674763C89AC566EC664159108E9(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(bool));
				bool L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		bool L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63078
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR bool AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m41529A6A47CC3B57286AC8F9C6388E54918DF450_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_mE3914411BE312CBBA869835158E535A06CD3EEB4(L_1, L_2, L_3, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		bool L_7;
		L_7 = AndroidJavaObject__CallStatic_TisBoolean_t09A6377A54BE2F9E6985A8149F19234FD7DDFE22_m9EDC162A865064239ADA4058CD2A81B1A2CF6D0F(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63079
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4F31B91D2F03C3068A4BE95DB368B6DF715CE316_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	int32_t V_3 = 0;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	int32_t V_5 = 0;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	int32_t G_B30_0 = 0;
	int32_t G_B35_0 = 0;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jclass;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallStaticIntMethod_m915549FA8FD7FB93B57A9708AD759488EA64418C(L_21, L_22, L_23, NULL);
				V_3 = L_24;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_25 = __this->___m_jclass;
				intptr_t L_26;
				L_26 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_25, NULL);
				intptr_t L_27 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_28 = V_0;
				bool L_29;
				L_29 = AndroidJNISafe_CallStaticBooleanMethod_m652685AC18F590965249C0F9B107C00C142595BB(L_26, L_27, L_28, NULL);
				bool L_30 = L_29;
				int32_t L_31;
				il2cpp_codegen_box_unbox((&L_30), (&L_31), sizeof(int32_t), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_31;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_32 = __this->___m_jclass;
				intptr_t L_33;
				L_33 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_32, NULL);
				intptr_t L_34 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_35 = V_0;
				int8_t L_36;
				L_36 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_33, L_34, L_35, NULL);
				uint8_t L_37 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_36,NULL));
				uint8_t L_38 = L_37;
				int32_t L_39;
				il2cpp_codegen_box_unbox((&L_38), (&L_39), sizeof(int32_t), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_39;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_40 = __this->___m_jclass;
				intptr_t L_41;
				L_41 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_40, NULL);
				intptr_t L_42 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_43 = V_0;
				int8_t L_44;
				L_44 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_41, L_42, L_43, NULL);
				int8_t L_45 = L_44;
				int32_t L_46;
				il2cpp_codegen_box_unbox((&L_45), (&L_46), sizeof(int32_t), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_46;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jclass;
				intptr_t L_48;
				L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
				intptr_t L_49 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_50 = V_0;
				int16_t L_51;
				L_51 = AndroidJNISafe_CallStaticShortMethod_m8330383670ECCD7E24CDD68C419745E486FA6426(L_48, L_49, L_50, NULL);
				int16_t L_52 = L_51;
				int32_t L_53;
				il2cpp_codegen_box_unbox((&L_52), (&L_53), sizeof(int32_t), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_53;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jclass;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int64_t L_58;
				L_58 = AndroidJNISafe_CallStaticLongMethod_mDDE01239BEFCF007ECE05E51A249B3EB5BB61234(L_55, L_56, L_57, NULL);
				int64_t L_59 = L_58;
				int32_t L_60;
				il2cpp_codegen_box_unbox((&L_59), (&L_60), sizeof(int32_t), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_60;
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jclass;
				intptr_t L_62;
				L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
				intptr_t L_63 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_64 = V_0;
				float L_65;
				L_65 = AndroidJNISafe_CallStaticFloatMethod_m3F5419A10B9DF599352938B2BAD8866F8F112364(L_62, L_63, L_64, NULL);
				float L_66 = L_65;
				int32_t L_67;
				il2cpp_codegen_box_unbox((&L_66), (&L_67), sizeof(int32_t), il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jclass;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallStaticDoubleMethod_m73F1D51601D6849EE480389B4E43AED68C42B2B5(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				int32_t L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(int32_t), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jclass;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallStaticCharMethod_mC4B40190CE095728E823AB8B724ECDC8F4B36155(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				int32_t L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(int32_t), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jclass;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStaticStringMethod_m4E150E34CC6DBF27A955F8DAEE5941D6E10879C0(L_83, L_84, L_85, NULL);
				V_3 = ((*(int32_t*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jclass;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(int32_t*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int32_t));
				int32_t L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jclass;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(int32_t*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int32_t));
				int32_t L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jclass;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_113, L_114, L_115, NULL);
				int32_t L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mC1390BDE8BF42FAA78215DFE076A25F7AEA674D7(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(int32_t));
				int32_t L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		int32_t L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63078
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mEE64E87A5A2BC0F38910643A9B91032AF38D882C_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mF87CC238DDBB3C90C35846E626548847CC7E319D(L_1, L_2, L_3, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		int32_t L_7;
		L_7 = AndroidJavaObject__CallStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4F31B91D2F03C3068A4BE95DB368B6DF715CE316(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63079
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m3F27BD182204628FAEA59AC70923AEB50F5B8B44_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	float V_3 = 0.0f;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	float V_5 = 0.0f;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	float G_B30_0 = 0.0f;
	float G_B35_0 = 0.0f;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jclass;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallStaticIntMethod_m915549FA8FD7FB93B57A9708AD759488EA64418C(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				float L_26;
				il2cpp_codegen_box_unbox((&L_25), (&L_26), sizeof(float), il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_26;
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jclass;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallStaticBooleanMethod_m652685AC18F590965249C0F9B107C00C142595BB(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				float L_33;
				il2cpp_codegen_box_unbox((&L_32), (&L_33), sizeof(float), il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_33;
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jclass;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				float L_41;
				il2cpp_codegen_box_unbox((&L_40), (&L_41), sizeof(float), il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_41;
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jclass;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				float L_48;
				il2cpp_codegen_box_unbox((&L_47), (&L_48), sizeof(float), il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_48;
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jclass;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallStaticShortMethod_m8330383670ECCD7E24CDD68C419745E486FA6426(L_50, L_51, L_52, NULL);
				int16_t L_54 = L_53;
				float L_55;
				il2cpp_codegen_box_unbox((&L_54), (&L_55), sizeof(float), il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_55;
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_56 = __this->___m_jclass;
				intptr_t L_57;
				L_57 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_56, NULL);
				intptr_t L_58 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_59 = V_0;
				int64_t L_60;
				L_60 = AndroidJNISafe_CallStaticLongMethod_mDDE01239BEFCF007ECE05E51A249B3EB5BB61234(L_57, L_58, L_59, NULL);
				int64_t L_61 = L_60;
				float L_62;
				il2cpp_codegen_box_unbox((&L_61), (&L_62), sizeof(float), il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_62;
				goto IL_03ea;
			}

IL_01cc_1:
			{
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_63 = __this->___m_jclass;
				intptr_t L_64;
				L_64 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_63, NULL);
				intptr_t L_65 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_66 = V_0;
				float L_67;
				L_67 = AndroidJNISafe_CallStaticFloatMethod_m3F5419A10B9DF599352938B2BAD8866F8F112364(L_64, L_65, L_66, NULL);
				V_3 = L_67;
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jclass;
				intptr_t L_69;
				L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
				intptr_t L_70 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_71 = V_0;
				double L_72;
				L_72 = AndroidJNISafe_CallStaticDoubleMethod_m73F1D51601D6849EE480389B4E43AED68C42B2B5(L_69, L_70, L_71, NULL);
				double L_73 = L_72;
				float L_74;
				il2cpp_codegen_box_unbox((&L_73), (&L_74), sizeof(float), il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_74;
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jclass;
				intptr_t L_76;
				L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
				intptr_t L_77 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_78 = V_0;
				Il2CppChar L_79;
				L_79 = AndroidJNISafe_CallStaticCharMethod_mC4B40190CE095728E823AB8B724ECDC8F4B36155(L_76, L_77, L_78, NULL);
				Il2CppChar L_80 = L_79;
				float L_81;
				il2cpp_codegen_box_unbox((&L_80), (&L_81), sizeof(float), il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				V_3 = L_81;
				goto IL_03ea;
			}

IL_0286_1:
			{
				goto IL_02be_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = __this->___m_jclass;
				intptr_t L_83;
				L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
				intptr_t L_84 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_85 = V_0;
				String_t* L_86;
				L_86 = AndroidJNISafe_CallStaticStringMethod_m4E150E34CC6DBF27A955F8DAEE5941D6E10879C0(L_83, L_84, L_85, NULL);
				V_3 = ((*(float*)UnBox((RuntimeObject*)L_86, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_03ea;
			}

IL_02be_1:
			{
				goto IL_0319_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jclass;
				intptr_t L_88;
				L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
				intptr_t L_89 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_90 = V_0;
				intptr_t L_91;
				L_91 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_88, L_89, L_90, NULL);
				V_4 = L_91;
				intptr_t L_92 = V_4;
				bool L_93;
				L_93 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_92, 0, NULL);
				if (L_93)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_94 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_95 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_95, L_94, NULL);
				G_B30_0 = ((*(float*)UnBox((RuntimeObject*)L_95, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(float));
				float L_96 = V_5;
				G_B30_0 = L_96;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				goto IL_0371_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_97 = __this->___m_jclass;
				intptr_t L_98;
				L_98 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_97, NULL);
				intptr_t L_99 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_100 = V_0;
				intptr_t L_101;
				L_101 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_98, L_99, L_100, NULL);
				V_6 = L_101;
				intptr_t L_102 = V_6;
				bool L_103;
				L_103 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_102, 0, NULL);
				if (L_103)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_104 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_105 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_105, L_104, NULL);
				G_B35_0 = ((*(float*)UnBox((RuntimeObject*)L_105, il2cpp_rgctx_data(method->rgctx_data, 1))));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(float));
				float L_106 = V_5;
				G_B35_0 = L_106;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_107 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_108;
				L_108 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_107, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_110;
				L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_111;
				L_111 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_108, L_110, NULL);
				if (!L_111)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_112 = __this->___m_jclass;
				intptr_t L_113;
				L_113 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_112, NULL);
				intptr_t L_114 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_115 = V_0;
				intptr_t L_116;
				L_116 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_113, L_114, L_115, NULL);
				float L_117;
				L_117 = AndroidJavaObject_FromJavaArray_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mD5A794B10305822230E288BE2D72329A6EE0C8A4(L_116, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_117;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_118 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_119;
				L_119 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_118, NULL);
				Type_t* L_120 = L_119;
				if (L_120)
				{
					G_B40_0 = L_120;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_120;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_121;
				L_121 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_121;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_122;
				L_122 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_123 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_123, L_122, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_123, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(float));
				float L_124 = V_5;
				V_3 = L_124;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		float L_125 = V_3;
		return L_125;
	}
}
// Method Definition Index: 63078
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mB41AF2746C52AC8CB15EFFA72BF98A34742B7690_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mA3FFDC6948922BEFC787A297D5483BA277EEFF0C(L_1, L_2, L_3, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		float L_7;
		L_7 = AndroidJavaObject__CallStatic_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m3F27BD182204628FAEA59AC70923AEB50F5B8B44(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63079
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_mF9D318147E29742DAA4B5C033BFC68D29CC1123B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	Il2CppSharedGenericObject* V_3 = NULL;
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	Il2CppSharedGenericObject* V_5 = NULL;
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	Il2CppSharedGenericObject* G_B30_0 = NULL;
	Il2CppSharedGenericObject* G_B35_0 = NULL;
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				goto IL_0090_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_20 = __this->___m_jclass;
				intptr_t L_21;
				L_21 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_20, NULL);
				intptr_t L_22 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_23 = V_0;
				int32_t L_24;
				L_24 = AndroidJNISafe_CallStaticIntMethod_m915549FA8FD7FB93B57A9708AD759488EA64418C(L_21, L_22, L_23, NULL);
				int32_t L_25 = L_24;
				RuntimeObject* L_26 = Box(il2cpp_defaults.int32_class, &L_25);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_26, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0090_1:
			{
				goto IL_00cd_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_27 = __this->___m_jclass;
				intptr_t L_28;
				L_28 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_27, NULL);
				intptr_t L_29 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_30 = V_0;
				bool L_31;
				L_31 = AndroidJNISafe_CallStaticBooleanMethod_m652685AC18F590965249C0F9B107C00C142595BB(L_28, L_29, L_30, NULL);
				bool L_32 = L_31;
				RuntimeObject* L_33 = Box(il2cpp_defaults.boolean_class, &L_32);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_33, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_00cd_1:
			{
				goto IL_0115_1;
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_34 = __this->___m_jclass;
				intptr_t L_35;
				L_35 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_34, NULL);
				intptr_t L_36 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_37 = V_0;
				int8_t L_38;
				L_38 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_35, L_36, L_37, NULL);
				uint8_t L_39 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_38,NULL));
				uint8_t L_40 = L_39;
				RuntimeObject* L_41 = Box(il2cpp_defaults.byte_class, &L_40);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_41, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0115_1:
			{
				goto IL_0152_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_42 = __this->___m_jclass;
				intptr_t L_43;
				L_43 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_42, NULL);
				intptr_t L_44 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_45 = V_0;
				int8_t L_46;
				L_46 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_43, L_44, L_45, NULL);
				int8_t L_47 = L_46;
				RuntimeObject* L_48 = Box(il2cpp_defaults.sbyte_class, &L_47);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_48, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0152_1:
			{
				goto IL_018f_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_49 = __this->___m_jclass;
				intptr_t L_50;
				L_50 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_49, NULL);
				intptr_t L_51 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_52 = V_0;
				int16_t L_53;
				L_53 = AndroidJNISafe_CallStaticShortMethod_m8330383670ECCD7E24CDD68C419745E486FA6426(L_50, L_51, L_52, NULL);
				int16_t L_54 = L_53;
				RuntimeObject* L_55 = Box(il2cpp_defaults.int16_class, &L_54);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_55, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_018f_1:
			{
				goto IL_01cc_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_56 = __this->___m_jclass;
				intptr_t L_57;
				L_57 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_56, NULL);
				intptr_t L_58 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_59 = V_0;
				int64_t L_60;
				L_60 = AndroidJNISafe_CallStaticLongMethod_mDDE01239BEFCF007ECE05E51A249B3EB5BB61234(L_57, L_58, L_59, NULL);
				int64_t L_61 = L_60;
				RuntimeObject* L_62 = Box(il2cpp_defaults.int64_class, &L_61);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_62, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_01cc_1:
			{
				goto IL_0209_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_63 = __this->___m_jclass;
				intptr_t L_64;
				L_64 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_63, NULL);
				intptr_t L_65 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_66 = V_0;
				float L_67;
				L_67 = AndroidJNISafe_CallStaticFloatMethod_m3F5419A10B9DF599352938B2BAD8866F8F112364(L_64, L_65, L_66, NULL);
				float L_68 = L_67;
				RuntimeObject* L_69 = Box(il2cpp_defaults.single_class, &L_68);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_69, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0209_1:
			{
				goto IL_0246_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_70 = __this->___m_jclass;
				intptr_t L_71;
				L_71 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_70, NULL);
				intptr_t L_72 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_73 = V_0;
				double L_74;
				L_74 = AndroidJNISafe_CallStaticDoubleMethod_m73F1D51601D6849EE480389B4E43AED68C42B2B5(L_71, L_72, L_73, NULL);
				double L_75 = L_74;
				RuntimeObject* L_76 = Box(il2cpp_defaults.double_class, &L_75);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_76, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0246_1:
			{
				goto IL_03d1_1;
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_77 = __this->___m_jclass;
				intptr_t L_78;
				L_78 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_77, NULL);
				intptr_t L_79 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_80 = V_0;
				Il2CppChar L_81;
				L_81 = AndroidJNISafe_CallStaticCharMethod_mC4B40190CE095728E823AB8B724ECDC8F4B36155(L_78, L_79, L_80, NULL);
				Il2CppChar L_82 = L_81;
				RuntimeObject* L_83 = Box(il2cpp_defaults.char_class, &L_82);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_83, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_0286_1:
			{
				bool L_84 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_84)
				{
					goto IL_02be_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_85 = __this->___m_jclass;
				intptr_t L_86;
				L_86 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_85, NULL);
				intptr_t L_87 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_88 = V_0;
				String_t* L_89;
				L_89 = AndroidJNISafe_CallStaticStringMethod_m4E150E34CC6DBF27A955F8DAEE5941D6E10879C0(L_86, L_87, L_88, NULL);
				V_3 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_89, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_03ea;
			}

IL_02be_1:
			{
				bool L_90 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_90)
				{
					goto IL_0319_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_91 = __this->___m_jclass;
				intptr_t L_92;
				L_92 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_91, NULL);
				intptr_t L_93 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_94 = V_0;
				intptr_t L_95;
				L_95 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_92, L_93, L_94, NULL);
				V_4 = L_95;
				intptr_t L_96 = V_4;
				bool L_97;
				L_97 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_96, 0, NULL);
				if (L_97)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_98 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_99 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_99, L_98, NULL);
				G_B30_0 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_99, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppSharedGenericObject*));
				Il2CppSharedGenericObject* L_100 = V_5;
				G_B30_0 = L_100;
			}

IL_0313_1:
			{
				V_3 = G_B30_0;
				goto IL_03ea;
			}

IL_0319_1:
			{
				bool L_101 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_101)
				{
					goto IL_0371_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_102 = __this->___m_jclass;
				intptr_t L_103;
				L_103 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_102, NULL);
				intptr_t L_104 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_105 = V_0;
				intptr_t L_106;
				L_106 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_103, L_104, L_105, NULL);
				V_6 = L_106;
				intptr_t L_107 = V_6;
				bool L_108;
				L_108 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_107, 0, NULL);
				if (L_108)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_109 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_110 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_110, L_109, NULL);
				G_B35_0 = ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_110, il2cpp_rgctx_data(method->rgctx_data, 1)));
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppSharedGenericObject*));
				Il2CppSharedGenericObject* L_111 = V_5;
				G_B35_0 = L_111;
			}

IL_036e_1:
			{
				V_3 = G_B35_0;
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_112 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_113;
				L_113 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_112, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_114 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_115;
				L_115 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_114, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_116;
				L_116 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_113, L_115, NULL);
				if (!L_116)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_117 = __this->___m_jclass;
				intptr_t L_118;
				L_118 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_117, NULL);
				intptr_t L_119 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_120 = V_0;
				intptr_t L_121;
				L_121 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_118, L_119, L_120, NULL);
				Il2CppSharedGenericObject* L_122;
				L_122 = AndroidJavaObject_FromJavaArray_TisIl2CppSharedGenericObject_m22D9898503E929D77563392822CD11A056613AFF(L_121, il2cpp_rgctx_method(method->rgctx_data, 2));
				V_3 = L_122;
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_123 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_124;
				L_124 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_123, NULL);
				Type_t* L_125 = L_124;
				if (L_125)
				{
					G_B40_0 = L_125;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_125;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_126;
				L_126 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_126;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_127;
				L_127 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_128 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_128, L_127, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_128, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((&V_5), sizeof(Il2CppSharedGenericObject*));
				Il2CppSharedGenericObject* L_129 = V_5;
				V_3 = L_129;
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		Il2CppSharedGenericObject* L_130 = V_3;
		return L_130;
	}
}
// Method Definition Index: 63078
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_m8E1513A9554AAA1822CA51D0A816DCF16775AA37_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = AndroidJNIHelper_GetMethodID_TisIl2CppSharedGenericObject_m95122CB55DFAF463B14CBBA51C2B0811790DB90B(L_1, L_2, L_3, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		Il2CppSharedGenericObject* L_7;
		L_7 = AndroidJavaObject__CallStatic_TisIl2CppSharedGenericObject_mF9D318147E29742DAA4B5C033BFC68D29CC1123B(__this, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_7;
	}
}
// Method Definition Index: 63079
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__CallStatic_TisIl2CppFullySharedGenericAny_m8D1EB1F6B7C8D0B7D54EF55F42BCE6B7576E0953_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_methodID, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_27 = alloca(SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	const Il2CppFullySharedGenericAny L_35 = L_27;
	const Il2CppFullySharedGenericAny L_44 = L_27;
	const Il2CppFullySharedGenericAny L_52 = L_27;
	const Il2CppFullySharedGenericAny L_60 = L_27;
	const Il2CppFullySharedGenericAny L_68 = L_27;
	const Il2CppFullySharedGenericAny L_76 = L_27;
	const Il2CppFullySharedGenericAny L_84 = L_27;
	const Il2CppFullySharedGenericAny L_92 = L_27;
	const Il2CppFullySharedGenericAny L_99 = L_27;
	const Il2CppFullySharedGenericAny L_111 = L_27;
	const Il2CppFullySharedGenericAny L_113 = L_27;
	const Il2CppFullySharedGenericAny L_124 = L_27;
	const Il2CppFullySharedGenericAny L_126 = L_27;
	const Il2CppFullySharedGenericAny L_137 = L_27;
	const Il2CppFullySharedGenericAny L_144 = L_27;
	const Il2CppFullySharedGenericAny L_145 = L_27;
	//<source_info:<no-source>:1>
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_0;
	memset((&V_0), 0, sizeof(V_0));
	Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 V_1;
	memset((&V_1), 0, sizeof(V_1));
	int32_t V_2 = 0;
	Il2CppFullySharedGenericAny V_3 = alloca(SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	memset(V_3, 0, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	intptr_t V_4;
	memset((&V_4), 0, sizeof(V_4));
	Il2CppFullySharedGenericAny V_5 = alloca(SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	memset(V_5, 0, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	intptr_t V_6;
	memset((&V_6), 0, sizeof(V_6));
	Il2CppFullySharedGenericAny G_B30_0 = alloca(SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	memset(G_B30_0, 0, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	Il2CppFullySharedGenericAny G_B35_0 = alloca(SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	memset(G_B35_0, 0, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_0 = ___1_args;
		if (!L_0)
		{
			goto IL_0007;
		}
	}
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_1 = ___1_args;
		NullCheck(L_1);
		if ((((RuntimeArray*)L_1)->max_length))
		{
			goto IL_0011;
		}
	}

IL_0007:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7));
		goto IL_0027;
	}

IL_0011:
	{
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_2 = ___1_args;
		NullCheck(L_2);
		int32_t L_3 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_2)->max_length),NULL));
		V_2 = L_3;
		int32_t L_4 = V_2;
		uintptr_t L_5 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(L_4,NULL));
		uint32_t L_6 = sizeof(jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225);
		if ((uintptr_t)L_5 * (uintptr_t)L_6 > (uintptr_t)kIl2CppUIntPtrMax)
			IL2CPP_RAISE_MANAGED_EXCEPTION(il2cpp_codegen_get_overflow_exception(), method);
		intptr_t L_7 = ((intptr_t)il2cpp_codegen_multiply((intptr_t)L_5, (int32_t)L_6));
		int8_t* L_8;
		if (L_7 == 0)
		{
			L_8 = NULL;
		}
		else
		{
			L_8 = (int8_t*)alloca(L_7);
			memset(L_8, 0, L_7);
		}
		int32_t L_9 = V_2;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_10;
		memset((&L_10), 0, sizeof(L_10));
		Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_inline((&L_10), (void*)L_8, L_9, Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_RuntimeMethod_var);
		V_1 = L_10;
	}

IL_0027:
	{
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_11 = V_1;
		V_0 = L_11;
		int32_t L_12;
		L_12 = Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_inline((&V_0), Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_RuntimeMethod_var);
		int32_t L_13;
		L_13 = AndroidJNI_PushLocalFrame_m2D8050A3799AEBB4A7E506E6790839EB66932E10(((int32_t)il2cpp_codegen_add(L_12, 1)), NULL);
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_14 = ___1_args;
		Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_15 = V_0;
		AndroidJNIHelper_CreateJNIArgArray_mD8E0CA2404E31F155EDE1A028EC686C17B17730F(L_14, L_15, NULL);
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_03de:
			{
				intptr_t L_16;
				L_16 = AndroidJNI_PopLocalFrame_m32AF6F9065F09D80BFDD3F573B62C782F392E609(0, NULL);
				return;
			}
		});
		try
		{
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_18;
				L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_19;
				L_19 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_18, NULL);
				if (!L_19)
				{
					goto IL_0286_1;
				}
			}
			{
				bool L_20 = (il2cpp_defaults.int32_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_20)
				{
					goto IL_0090_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_21 = __this->___m_jclass;
				intptr_t L_22;
				L_22 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_21, NULL);
				intptr_t L_23 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_24 = V_0;
				int32_t L_25;
				L_25 = AndroidJNISafe_CallStaticIntMethod_m915549FA8FD7FB93B57A9708AD759488EA64418C(L_22, L_23, L_24, NULL);
				int32_t L_26 = L_25;
				il2cpp_codegen_box_unbox((&L_26), L_27, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_27, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0090_1:
			{
				bool L_28 = (il2cpp_defaults.boolean_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_28)
				{
					goto IL_00cd_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_29 = __this->___m_jclass;
				intptr_t L_30;
				L_30 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_29, NULL);
				intptr_t L_31 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_32 = V_0;
				bool L_33;
				L_33 = AndroidJNISafe_CallStaticBooleanMethod_m652685AC18F590965249C0F9B107C00C142595BB(L_30, L_31, L_32, NULL);
				bool L_34 = L_33;
				il2cpp_codegen_box_unbox((&L_34), L_35, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_35, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_00cd_1:
			{
				bool L_36 = (il2cpp_defaults.byte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_36)
				{
					goto IL_0115_1;
				}
			}
			{
				il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
				Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteralBBA88D18E0EF50828EC393CF2F0ADD9E557A1A9E, NULL);
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_37 = __this->___m_jclass;
				intptr_t L_38;
				L_38 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_37, NULL);
				intptr_t L_39 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_40 = V_0;
				int8_t L_41;
				L_41 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_38, L_39, L_40, NULL);
				uint8_t L_42 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_41,NULL));
				uint8_t L_43 = L_42;
				il2cpp_codegen_box_unbox((&L_43), L_44, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_44, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0115_1:
			{
				bool L_45 = (il2cpp_defaults.sbyte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_45)
				{
					goto IL_0152_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_46 = __this->___m_jclass;
				intptr_t L_47;
				L_47 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_46, NULL);
				intptr_t L_48 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_49 = V_0;
				int8_t L_50;
				L_50 = AndroidJNISafe_CallStaticSByteMethod_m3E1F75978A2D686BC32DBF5A2F1F70F0D746C2B7(L_47, L_48, L_49, NULL);
				int8_t L_51 = L_50;
				il2cpp_codegen_box_unbox((&L_51), L_52, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_52, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0152_1:
			{
				bool L_53 = (il2cpp_defaults.int16_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_53)
				{
					goto IL_018f_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jclass;
				intptr_t L_55;
				L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
				intptr_t L_56 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_57 = V_0;
				int16_t L_58;
				L_58 = AndroidJNISafe_CallStaticShortMethod_m8330383670ECCD7E24CDD68C419745E486FA6426(L_55, L_56, L_57, NULL);
				int16_t L_59 = L_58;
				il2cpp_codegen_box_unbox((&L_59), L_60, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_60, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_018f_1:
			{
				bool L_61 = (il2cpp_defaults.int64_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_61)
				{
					goto IL_01cc_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_62 = __this->___m_jclass;
				intptr_t L_63;
				L_63 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_62, NULL);
				intptr_t L_64 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_65 = V_0;
				int64_t L_66;
				L_66 = AndroidJNISafe_CallStaticLongMethod_mDDE01239BEFCF007ECE05E51A249B3EB5BB61234(L_63, L_64, L_65, NULL);
				int64_t L_67 = L_66;
				il2cpp_codegen_box_unbox((&L_67), L_68, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_68, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_01cc_1:
			{
				bool L_69 = (il2cpp_defaults.single_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_69)
				{
					goto IL_0209_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_70 = __this->___m_jclass;
				intptr_t L_71;
				L_71 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_70, NULL);
				intptr_t L_72 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_73 = V_0;
				float L_74;
				L_74 = AndroidJNISafe_CallStaticFloatMethod_m3F5419A10B9DF599352938B2BAD8866F8F112364(L_71, L_72, L_73, NULL);
				float L_75 = L_74;
				il2cpp_codegen_box_unbox((&L_75), L_76, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_76, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0209_1:
			{
				bool L_77 = (il2cpp_defaults.double_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_77)
				{
					goto IL_0246_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_78 = __this->___m_jclass;
				intptr_t L_79;
				L_79 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_78, NULL);
				intptr_t L_80 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_81 = V_0;
				double L_82;
				L_82 = AndroidJNISafe_CallStaticDoubleMethod_m73F1D51601D6849EE480389B4E43AED68C42B2B5(L_79, L_80, L_81, NULL);
				double L_83 = L_82;
				il2cpp_codegen_box_unbox((&L_83), L_84, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_84, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0246_1:
			{
				bool L_85 = (il2cpp_defaults.char_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_85)
				{
					goto IL_03d1_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_86 = __this->___m_jclass;
				intptr_t L_87;
				L_87 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_86, NULL);
				intptr_t L_88 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_89 = V_0;
				Il2CppChar L_90;
				L_90 = AndroidJNISafe_CallStaticCharMethod_mC4B40190CE095728E823AB8B724ECDC8F4B36155(L_87, L_88, L_89, NULL);
				Il2CppChar L_91 = L_90;
				il2cpp_codegen_box_unbox((&L_91), L_92, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B, il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
				il2cpp_codegen_memcpy(V_3, L_92, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0286_1:
			{
				bool L_93 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_93)
				{
					goto IL_02be_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_94 = __this->___m_jclass;
				intptr_t L_95;
				L_95 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_94, NULL);
				intptr_t L_96 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_97 = V_0;
				String_t* L_98;
				L_98 = AndroidJNISafe_CallStaticStringMethod_m4E150E34CC6DBF27A955F8DAEE5941D6E10879C0(L_95, L_96, L_97, NULL);
				void* L_100 = UnBox_Any((RuntimeObject*)L_98, il2cpp_rgctx_data(method->rgctx_data, 1), L_99);
				il2cpp_codegen_memcpy(V_3, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_100)), SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_02be_1:
			{
				bool L_101 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_101)
				{
					goto IL_0319_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_102 = __this->___m_jclass;
				intptr_t L_103;
				L_103 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_102, NULL);
				intptr_t L_104 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_105 = V_0;
				intptr_t L_106;
				L_106 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_103, L_104, L_105, NULL);
				V_4 = L_106;
				intptr_t L_107 = V_4;
				bool L_108;
				L_108 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_107, 0, NULL);
				if (L_108)
				{
					goto IL_0309_1;
				}
			}
			{
				intptr_t L_109 = V_4;
				AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_110 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)il2cpp_codegen_object_new(AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
				AndroidJavaClass__ctor_mB206D3CB990755BD56E308F61CD43BB9EA4421D0(L_110, L_109, NULL);
				void* L_112 = UnBox_Any((RuntimeObject*)L_110, il2cpp_rgctx_data(method->rgctx_data, 1), L_111);
				il2cpp_codegen_memcpy(G_B30_0, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_112)), SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_0313_1;
			}

IL_0309_1:
			{
				il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_5, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				il2cpp_codegen_memcpy(L_113, V_5, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				il2cpp_codegen_memcpy(G_B30_0, L_113, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
			}

IL_0313_1:
			{
				il2cpp_codegen_memcpy(V_3, G_B30_0, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0319_1:
			{
				bool L_114 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
				if (!L_114)
				{
					goto IL_0371_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_115 = __this->___m_jclass;
				intptr_t L_116;
				L_116 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_115, NULL);
				intptr_t L_117 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_118 = V_0;
				intptr_t L_119;
				L_119 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_116, L_117, L_118, NULL);
				V_6 = L_119;
				intptr_t L_120 = V_6;
				bool L_121;
				L_121 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_120, 0, NULL);
				if (L_121)
				{
					goto IL_0364_1;
				}
			}
			{
				intptr_t L_122 = V_6;
				AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_123 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)il2cpp_codegen_object_new(AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
				AndroidJavaObject__ctor_m0CEE7D570807333CE2C193A82AB3AB8D4F873A6B(L_123, L_122, NULL);
				void* L_125 = UnBox_Any((RuntimeObject*)L_123, il2cpp_rgctx_data(method->rgctx_data, 1), L_124);
				il2cpp_codegen_memcpy(G_B35_0, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_125)), SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_036e_1;
			}

IL_0364_1:
			{
				il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_5, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				il2cpp_codegen_memcpy(L_126, V_5, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				il2cpp_codegen_memcpy(G_B35_0, L_126, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
			}

IL_036e_1:
			{
				il2cpp_codegen_memcpy(V_3, G_B35_0, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_0371_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_127 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_128;
				L_128 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_127, NULL);
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_129 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				Type_t* L_130;
				L_130 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_129, NULL);
				il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
				bool L_131;
				L_131 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_128, L_130, NULL);
				if (!L_131)
				{
					goto IL_03a6_1;
				}
			}
			{
				GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_132 = __this->___m_jclass;
				intptr_t L_133;
				L_133 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_132, NULL);
				intptr_t L_134 = ___0_methodID;
				Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7 L_135 = V_0;
				intptr_t L_136;
				L_136 = AndroidJNISafe_CallStaticObjectMethod_m3171BFAEF780EEF400AD592B6F040E7BE87C2387(L_133, L_134, L_135, NULL);
				InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_136, (Il2CppFullySharedGenericAny*)L_137);
				il2cpp_codegen_memcpy(V_3, L_137, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}

IL_03a6_1:
			{
				RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_138 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
				il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
				Type_t* L_139;
				L_139 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_138, NULL);
				Type_t* L_140 = L_139;
				if (L_140)
				{
					G_B40_0 = L_140;
					G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
					goto IL_03bc_1;
				}
				G_B39_0 = L_140;
				G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralD89F6478390C0E72E54E8EB2729A4BBC7009F43D));
			}
			{
				G_B41_0 = ((String_t*)(NULL));
				G_B41_1 = G_B39_1;
				goto IL_03c1_1;
			}

IL_03bc_1:
			{
				NullCheck((RuntimeObject*)G_B40_0);
				String_t* L_141;
				L_141 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
				G_B41_0 = L_141;
				G_B41_1 = G_B40_1;
			}

IL_03c1_1:
			{
				String_t* L_142;
				L_142 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
				Exception_t* L_143 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
				Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_143, L_142, NULL);
				IL2CPP_RAISE_MANAGED_EXCEPTION(L_143, method);
			}

IL_03d1_1:
			{
				il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_5, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				il2cpp_codegen_memcpy(L_144, V_5, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				il2cpp_codegen_memcpy(V_3, L_144, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
				goto IL_03ea;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_03ea:
	{
		il2cpp_codegen_memcpy(L_145, V_3, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
		il2cpp_codegen_memcpy(il2cppRetVal, L_145, SizeOf_ReturnType_tB840B385B71FF8E350FF799B5A7DF58E03C1A69B);
		return;
	}
}
// Method Definition Index: 63078
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__CallStatic_TisIl2CppFullySharedGenericAny_mA069934E49E8EFEB980CA5EED2B1DE810C150A68_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_methodName, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* ___1_args, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_ReturnType_t2DAB0875DF34A21B532F695F9B7329A0B5BBCB27 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_7 = alloca(SizeOf_ReturnType_t2DAB0875DF34A21B532F695F9B7329A0B5BBCB27);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_methodName;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_3 = ___1_args;
		intptr_t L_4;
		L_4 = ((  intptr_t (*) (intptr_t, String_t*, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_1, L_2, L_3, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_4;
		intptr_t L_5 = V_0;
		ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918* L_6 = ___1_args;
		InvokerActionInvoker3< intptr_t, ObjectU5BU5D_t8061030B0A12A55D5AD8652A20C922FE99450918*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_5, L_6, (Il2CppFullySharedGenericAny*)L_7);
		il2cpp_codegen_memcpy(il2cppRetVal, L_7, SizeOf_ReturnType_t2DAB0875DF34A21B532F695F9B7329A0B5BBCB27);
		return;
	}
}
// Method Definition Index: 63073
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB506BB319D4A193AE597F3C3166DDB165593D4D6_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	int32_t V_1 = 0;
	intptr_t V_2;
	memset((&V_2), 0, sizeof(V_2));
	Type_t* G_B33_0 = NULL;
	String_t* G_B33_1 = NULL;
	Type_t* G_B32_0 = NULL;
	String_t* G_B32_1 = NULL;
	String_t* G_B34_0 = NULL;
	String_t* G_B34_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_0211;
		}
	}
	{
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_3 = __this->___m_jobject;
		intptr_t L_4;
		L_4 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_3, NULL);
		intptr_t L_5 = ___0_fieldID;
		int32_t L_6;
		L_6 = AndroidJNISafe_GetIntField_mBD983688B73063DE5C55D320F60F266443FAC97C(L_4, L_5, NULL);
		return L_6;
	}

IL_0211:
	{
		goto IL_0243;
	}

IL_0243:
	{
		goto IL_0293;
	}

IL_0293:
	{
		goto IL_02e3;
	}

IL_02e3:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_7 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_8;
		L_8 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_7, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_9 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_10;
		L_10 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_9, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_11;
		L_11 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_8, L_10, NULL);
		if (!L_11)
		{
			goto IL_0315;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_12 = __this->___m_jobject;
		intptr_t L_13;
		L_13 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_12, NULL);
		intptr_t L_14 = ___0_fieldID;
		intptr_t L_15;
		L_15 = AndroidJNISafe_GetObjectField_mCF3BB1C38718D6F55081126BC7F6C286B382B275(L_13, L_14, NULL);
		int32_t L_16;
		L_16 = AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m61DD76EF7E812BC3ED41BBA0D0DA1B430EC61256(L_15, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_16;
	}

IL_0315:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_18;
		L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
		Type_t* L_19 = L_18;
		if (L_19)
		{
			G_B33_0 = L_19;
			G_B33_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_032b;
		}
		G_B32_0 = L_19;
		G_B32_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B34_0 = ((String_t*)(NULL));
		G_B34_1 = G_B32_1;
		goto IL_0330;
	}

IL_032b:
	{
		NullCheck((RuntimeObject*)G_B33_0);
		String_t* L_20;
		L_20 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B33_0);
		G_B34_0 = L_20;
		G_B34_1 = G_B33_1;
	}

IL_0330:
	{
		String_t* L_21;
		L_21 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B34_1, G_B34_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_22 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_22, L_21, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_22, method);
	}
	IL2CPP_UNREACHABLE;
	int32_t ret = 0;
	return ret;
}
// Method Definition Index: 63072
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mDED207A2CABEE9D78BEC022F1D4338A5091DCC50_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1B394DCE5ED40DFA09EB18874BA2FFC3B5E50B2E(L_1, L_2, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		int32_t L_5;
		L_5 = AndroidJavaObject__Get_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_mB506BB319D4A193AE597F3C3166DDB165593D4D6(__this, L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_5;
	}
}
// Method Definition Index: 63073
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m0EB49046BB127E247EF96EA1C8BB5BC0D4F4B439_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	float V_1 = 0.0f;
	intptr_t V_2;
	memset((&V_2), 0, sizeof(V_2));
	Type_t* G_B33_0 = NULL;
	String_t* G_B33_1 = NULL;
	Type_t* G_B32_0 = NULL;
	String_t* G_B32_1 = NULL;
	String_t* G_B34_0 = NULL;
	String_t* G_B34_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_0211;
		}
	}
	{
		goto IL_004b;
	}

IL_004b:
	{
		goto IL_0082;
	}

IL_0082:
	{
		goto IL_00c4;
	}

IL_00c4:
	{
		goto IL_00fb;
	}

IL_00fb:
	{
		goto IL_0132;
	}

IL_0132:
	{
		goto IL_0169;
	}

IL_0169:
	{
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_3 = __this->___m_jobject;
		intptr_t L_4;
		L_4 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_3, NULL);
		intptr_t L_5 = ___0_fieldID;
		float L_6;
		L_6 = AndroidJNISafe_GetFloatField_m1EAA1ED33002BBA28CA2B630521D6BF1B7D3A2E7(L_4, L_5, NULL);
		return L_6;
	}

IL_0211:
	{
		goto IL_0243;
	}

IL_0243:
	{
		goto IL_0293;
	}

IL_0293:
	{
		goto IL_02e3;
	}

IL_02e3:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_7 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_8;
		L_8 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_7, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_9 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_10;
		L_10 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_9, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_11;
		L_11 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_8, L_10, NULL);
		if (!L_11)
		{
			goto IL_0315;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_12 = __this->___m_jobject;
		intptr_t L_13;
		L_13 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_12, NULL);
		intptr_t L_14 = ___0_fieldID;
		intptr_t L_15;
		L_15 = AndroidJNISafe_GetObjectField_mCF3BB1C38718D6F55081126BC7F6C286B382B275(L_13, L_14, NULL);
		float L_16;
		L_16 = AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m6191F97DB89FB7E398F8A3DF9E448A9DCD5F07EA(L_15, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_16;
	}

IL_0315:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_18;
		L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
		Type_t* L_19 = L_18;
		if (L_19)
		{
			G_B33_0 = L_19;
			G_B33_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_032b;
		}
		G_B32_0 = L_19;
		G_B32_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B34_0 = ((String_t*)(NULL));
		G_B34_1 = G_B32_1;
		goto IL_0330;
	}

IL_032b:
	{
		NullCheck((RuntimeObject*)G_B33_0);
		String_t* L_20;
		L_20 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B33_0);
		G_B34_0 = L_20;
		G_B34_1 = G_B33_1;
	}

IL_0330:
	{
		String_t* L_21;
		L_21 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B34_1, G_B34_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_22 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_22, L_21, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_22, method);
	}
	IL2CPP_UNREACHABLE;
	float ret = 0.0f;
	return ret;
}
// Method Definition Index: 63072
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR float AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m599A48B8B43BD171290DD40A653B639B6F856D42_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = AndroidJNIHelper_GetFieldID_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_mDD609864F766F77FB093EC507721536574D38177(L_1, L_2, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		float L_5;
		L_5 = AndroidJavaObject__Get_TisSingle_t4530F2FF86FCB0DC29F35385CA1BD21BE294761C_m0EB49046BB127E247EF96EA1C8BB5BC0D4F4B439(__this, L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_5;
	}
}
// Method Definition Index: 63073
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Get_TisIl2CppFullySharedGenericAny_m16AE597DA478C33ABF978BB8131126B5C7360D09_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral50E02893F5B6939534A49C535241D6B7BAFCF599);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_9 = alloca(SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
	const Il2CppFullySharedGenericAny L_16 = L_9;
	const Il2CppFullySharedGenericAny L_24 = L_9;
	const Il2CppFullySharedGenericAny L_31 = L_9;
	const Il2CppFullySharedGenericAny L_38 = L_9;
	const Il2CppFullySharedGenericAny L_45 = L_9;
	const Il2CppFullySharedGenericAny L_52 = L_9;
	const Il2CppFullySharedGenericAny L_59 = L_9;
	const Il2CppFullySharedGenericAny L_66 = L_9;
	const Il2CppFullySharedGenericAny L_72 = L_9;
	const Il2CppFullySharedGenericAny L_83 = L_9;
	const Il2CppFullySharedGenericAny L_85 = L_9;
	const Il2CppFullySharedGenericAny L_95 = L_9;
	const Il2CppFullySharedGenericAny L_97 = L_9;
	const Il2CppFullySharedGenericAny L_107 = L_9;
	const Il2CppFullySharedGenericAny L_114 = L_9;
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	Il2CppFullySharedGenericAny V_1 = alloca(SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
	memset(V_1, 0, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
	intptr_t V_2;
	memset((&V_2), 0, sizeof(V_2));
	Type_t* G_B33_0 = NULL;
	String_t* G_B33_1 = NULL;
	Type_t* G_B32_0 = NULL;
	String_t* G_B32_1 = NULL;
	String_t* G_B34_0 = NULL;
	String_t* G_B34_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_0211;
		}
	}
	{
		bool L_3 = (il2cpp_defaults.int32_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_3)
		{
			goto IL_004b;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_4 = __this->___m_jobject;
		intptr_t L_5;
		L_5 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_4, NULL);
		intptr_t L_6 = ___0_fieldID;
		int32_t L_7;
		L_7 = AndroidJNISafe_GetIntField_mBD983688B73063DE5C55D320F60F266443FAC97C(L_5, L_6, NULL);
		int32_t L_8 = L_7;
		il2cpp_codegen_box_unbox((&L_8), L_9, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_9, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_004b:
	{
		bool L_10 = (il2cpp_defaults.boolean_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_10)
		{
			goto IL_0082;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_11 = __this->___m_jobject;
		intptr_t L_12;
		L_12 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_11, NULL);
		intptr_t L_13 = ___0_fieldID;
		bool L_14;
		L_14 = AndroidJNISafe_GetBooleanField_m34F37B560A6AEC81B9061FB3B72698C84720435D(L_12, L_13, NULL);
		bool L_15 = L_14;
		il2cpp_codegen_box_unbox((&L_15), L_16, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_16, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0082:
	{
		bool L_17 = (il2cpp_defaults.byte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_17)
		{
			goto IL_00c4;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteral50E02893F5B6939534A49C535241D6B7BAFCF599, NULL);
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_18 = __this->___m_jobject;
		intptr_t L_19;
		L_19 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_18, NULL);
		intptr_t L_20 = ___0_fieldID;
		int8_t L_21;
		L_21 = AndroidJNISafe_GetSByteField_mAD3B08AA8A97F77CAE17DD25B0F389AFAC2023B1(L_19, L_20, NULL);
		uint8_t L_22 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_21,NULL));
		uint8_t L_23 = L_22;
		il2cpp_codegen_box_unbox((&L_23), L_24, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_24, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_00c4:
	{
		bool L_25 = (il2cpp_defaults.sbyte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_25)
		{
			goto IL_00fb;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_26 = __this->___m_jobject;
		intptr_t L_27;
		L_27 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_26, NULL);
		intptr_t L_28 = ___0_fieldID;
		int8_t L_29;
		L_29 = AndroidJNISafe_GetSByteField_mAD3B08AA8A97F77CAE17DD25B0F389AFAC2023B1(L_27, L_28, NULL);
		int8_t L_30 = L_29;
		il2cpp_codegen_box_unbox((&L_30), L_31, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_31, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_00fb:
	{
		bool L_32 = (il2cpp_defaults.int16_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_32)
		{
			goto IL_0132;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_33 = __this->___m_jobject;
		intptr_t L_34;
		L_34 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_33, NULL);
		intptr_t L_35 = ___0_fieldID;
		int16_t L_36;
		L_36 = AndroidJNISafe_GetShortField_m5D21E87061C1DAC89DF58671C53432D0361F0C6E(L_34, L_35, NULL);
		int16_t L_37 = L_36;
		il2cpp_codegen_box_unbox((&L_37), L_38, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_38, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0132:
	{
		bool L_39 = (il2cpp_defaults.int64_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_39)
		{
			goto IL_0169;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_40 = __this->___m_jobject;
		intptr_t L_41;
		L_41 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_40, NULL);
		intptr_t L_42 = ___0_fieldID;
		int64_t L_43;
		L_43 = AndroidJNISafe_GetLongField_m7DD751358D10BB276D8A95D413B9DFB1E8EE81D8(L_41, L_42, NULL);
		int64_t L_44 = L_43;
		il2cpp_codegen_box_unbox((&L_44), L_45, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_45, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0169:
	{
		bool L_46 = (il2cpp_defaults.single_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_46)
		{
			goto IL_01a0;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jobject;
		intptr_t L_48;
		L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
		intptr_t L_49 = ___0_fieldID;
		float L_50;
		L_50 = AndroidJNISafe_GetFloatField_m1EAA1ED33002BBA28CA2B630521D6BF1B7D3A2E7(L_48, L_49, NULL);
		float L_51 = L_50;
		il2cpp_codegen_box_unbox((&L_51), L_52, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_52, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_01a0:
	{
		bool L_53 = (il2cpp_defaults.double_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_53)
		{
			goto IL_01d7;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jobject;
		intptr_t L_55;
		L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
		intptr_t L_56 = ___0_fieldID;
		double L_57;
		L_57 = AndroidJNISafe_GetDoubleField_mBCBD5E80223EDECC06FA783F34149E3625219074(L_55, L_56, NULL);
		double L_58 = L_57;
		il2cpp_codegen_box_unbox((&L_58), L_59, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_59, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_01d7:
	{
		bool L_60 = (il2cpp_defaults.char_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_60)
		{
			goto IL_0340;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jobject;
		intptr_t L_62;
		L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
		intptr_t L_63 = ___0_fieldID;
		Il2CppChar L_64;
		L_64 = AndroidJNISafe_GetCharField_m8301FA96B40E27C032590FE3F8E84A777A4739C3(L_62, L_63, NULL);
		Il2CppChar L_65 = L_64;
		il2cpp_codegen_box_unbox((&L_65), L_66, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29, il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_66, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0211:
	{
		bool L_67 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_67)
		{
			goto IL_0243;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jobject;
		intptr_t L_69;
		L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
		intptr_t L_70 = ___0_fieldID;
		String_t* L_71;
		L_71 = AndroidJNISafe_GetStringField_mADFCA05D6DE790600B57E90B20F2E75AFC036B0F(L_69, L_70, NULL);
		void* L_73 = UnBox_Any((RuntimeObject*)L_71, il2cpp_rgctx_data(method->rgctx_data, 1), L_72);
		il2cpp_codegen_memcpy(il2cppRetVal, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_73)), SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0243:
	{
		bool L_74 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_74)
		{
			goto IL_0293;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
		intptr_t L_76;
		L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
		intptr_t L_77 = ___0_fieldID;
		intptr_t L_78;
		L_78 = AndroidJNISafe_GetObjectField_mCF3BB1C38718D6F55081126BC7F6C286B382B275(L_76, L_77, NULL);
		V_0 = L_78;
		intptr_t L_79 = V_0;
		bool L_80;
		L_80 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_79, 0, NULL);
		if (L_80)
		{
			goto IL_0289;
		}
	}
	{
		intptr_t L_81 = V_0;
		AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_82;
		L_82 = AndroidJavaObject_AndroidJavaClassDeleteLocalRef_m56C84D7516BCB51A84E8AFDB3FCA46BAF494548F(L_81, NULL);
		void* L_84 = UnBox_Any((RuntimeObject*)L_82, il2cpp_rgctx_data(method->rgctx_data, 1), L_83);
		il2cpp_codegen_memcpy(il2cppRetVal, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_84)), SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0289:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_1, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		il2cpp_codegen_memcpy(L_85, V_1, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		il2cpp_codegen_memcpy(il2cppRetVal, L_85, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0293:
	{
		bool L_86 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_86)
		{
			goto IL_02e3;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jobject;
		intptr_t L_88;
		L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
		intptr_t L_89 = ___0_fieldID;
		intptr_t L_90;
		L_90 = AndroidJNISafe_GetObjectField_mCF3BB1C38718D6F55081126BC7F6C286B382B275(L_88, L_89, NULL);
		V_2 = L_90;
		intptr_t L_91 = V_2;
		bool L_92;
		L_92 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_91, 0, NULL);
		if (L_92)
		{
			goto IL_02d9;
		}
	}
	{
		intptr_t L_93 = V_2;
		AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_94;
		L_94 = AndroidJavaObject_AndroidJavaObjectDeleteLocalRef_m2ECEEAF6389ABB9D6B963B8A98568ECD9413DF3C(L_93, NULL);
		void* L_96 = UnBox_Any((RuntimeObject*)L_94, il2cpp_rgctx_data(method->rgctx_data, 1), L_95);
		il2cpp_codegen_memcpy(il2cppRetVal, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_96)), SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_02d9:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_1, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		il2cpp_codegen_memcpy(L_97, V_1, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		il2cpp_codegen_memcpy(il2cppRetVal, L_97, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_02e3:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_98 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_99;
		L_99 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_98, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_100 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_101;
		L_101 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_100, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_102;
		L_102 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_99, L_101, NULL);
		if (!L_102)
		{
			goto IL_0315;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_103 = __this->___m_jobject;
		intptr_t L_104;
		L_104 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_103, NULL);
		intptr_t L_105 = ___0_fieldID;
		intptr_t L_106;
		L_106 = AndroidJNISafe_GetObjectField_mCF3BB1C38718D6F55081126BC7F6C286B382B275(L_104, L_105, NULL);
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_106, (Il2CppFullySharedGenericAny*)L_107);
		il2cpp_codegen_memcpy(il2cppRetVal, L_107, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}

IL_0315:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_108 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_109;
		L_109 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_108, NULL);
		Type_t* L_110 = L_109;
		if (L_110)
		{
			G_B33_0 = L_110;
			G_B33_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_032b;
		}
		G_B32_0 = L_110;
		G_B32_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B34_0 = ((String_t*)(NULL));
		G_B34_1 = G_B32_1;
		goto IL_0330;
	}

IL_032b:
	{
		NullCheck((RuntimeObject*)G_B33_0);
		String_t* L_111;
		L_111 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B33_0);
		G_B34_0 = L_111;
		G_B34_1 = G_B33_1;
	}

IL_0330:
	{
		String_t* L_112;
		L_112 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B34_1, G_B34_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_113 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_113, L_112, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_113, method);
	}

IL_0340:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_1, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		il2cpp_codegen_memcpy(L_114, V_1, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		il2cpp_codegen_memcpy(il2cppRetVal, L_114, SizeOf_FieldType_t1266B1AF802837275B234C725395054CAC857D29);
		return;
	}
}
// Method Definition Index: 63072
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Get_TisIl2CppFullySharedGenericAny_m6AA7C8A5BE069836640026CB6EDA5D93B3246683_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_tBD81DD51AB3076BC0134BD255EBBAC34E341C535 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_FieldType_tBD81DD51AB3076BC0134BD255EBBAC34E341C535);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_1, L_2, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_4, (Il2CppFullySharedGenericAny*)L_5);
		il2cpp_codegen_memcpy(il2cppRetVal, L_5, SizeOf_FieldType_tBD81DD51AB3076BC0134BD255EBBAC34E341C535);
		return;
	}
}
// Method Definition Index: 63081
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m86131C9F9AC1A6082963096E96DDDDDD7366189E_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	int32_t V_1 = 0;
	intptr_t V_2;
	memset((&V_2), 0, sizeof(V_2));
	Type_t* G_B33_0 = NULL;
	String_t* G_B33_1 = NULL;
	Type_t* G_B32_0 = NULL;
	String_t* G_B32_1 = NULL;
	String_t* G_B34_0 = NULL;
	String_t* G_B34_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_0211;
		}
	}
	{
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_3 = __this->___m_jclass;
		intptr_t L_4;
		L_4 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_3, NULL);
		intptr_t L_5 = ___0_fieldID;
		int32_t L_6;
		L_6 = AndroidJNISafe_GetStaticIntField_m0698D50C44E490A009E8388C7321630DED5973BD(L_4, L_5, NULL);
		return L_6;
	}

IL_0211:
	{
		goto IL_0243;
	}

IL_0243:
	{
		goto IL_0293;
	}

IL_0293:
	{
		goto IL_02e3;
	}

IL_02e3:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_7 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_8;
		L_8 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_7, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_9 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_10;
		L_10 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_9, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_11;
		L_11 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_8, L_10, NULL);
		if (!L_11)
		{
			goto IL_0315;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_12 = __this->___m_jclass;
		intptr_t L_13;
		L_13 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_12, NULL);
		intptr_t L_14 = ___0_fieldID;
		intptr_t L_15;
		L_15 = AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867(L_13, L_14, NULL);
		int32_t L_16;
		L_16 = AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m61DD76EF7E812BC3ED41BBA0D0DA1B430EC61256(L_15, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_16;
	}

IL_0315:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_17 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_18;
		L_18 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_17, NULL);
		Type_t* L_19 = L_18;
		if (L_19)
		{
			G_B33_0 = L_19;
			G_B33_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_032b;
		}
		G_B32_0 = L_19;
		G_B32_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B34_0 = ((String_t*)(NULL));
		G_B34_1 = G_B32_1;
		goto IL_0330;
	}

IL_032b:
	{
		NullCheck((RuntimeObject*)G_B33_0);
		String_t* L_20;
		L_20 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B33_0);
		G_B34_0 = L_20;
		G_B34_1 = G_B33_1;
	}

IL_0330:
	{
		String_t* L_21;
		L_21 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B34_1, G_B34_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_22 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_22, L_21, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_22, method);
	}
	IL2CPP_UNREACHABLE;
	int32_t ret = 0;
	return ret;
}
// Method Definition Index: 63080
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m4289B7D740963C5007AD93A07FF6A0A01857DD28_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = AndroidJNIHelper_GetFieldID_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m1B394DCE5ED40DFA09EB18874BA2FFC3B5E50B2E(L_1, L_2, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		int32_t L_5;
		L_5 = AndroidJavaObject__GetStatic_TisInt32_t680FF22E76F6EFAD4375103CBBFFA0421349384C_m86131C9F9AC1A6082963096E96DDDDDD7366189E(__this, L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_5;
	}
}
// Method Definition Index: 63081
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_mF12B3A136CBDB98DF0876DF1FC47EA4A14D98A17_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	Il2CppSharedGenericObject* V_1 = NULL;
	intptr_t V_2;
	memset((&V_2), 0, sizeof(V_2));
	Type_t* G_B33_0 = NULL;
	String_t* G_B33_1 = NULL;
	Type_t* G_B32_0 = NULL;
	String_t* G_B32_1 = NULL;
	String_t* G_B34_0 = NULL;
	String_t* G_B34_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_0211;
		}
	}
	{
		goto IL_004b;
	}

IL_004b:
	{
		goto IL_0082;
	}

IL_0082:
	{
		goto IL_00c4;
	}

IL_00c4:
	{
		goto IL_00fb;
	}

IL_00fb:
	{
		goto IL_0132;
	}

IL_0132:
	{
		goto IL_0169;
	}

IL_0169:
	{
		goto IL_01a0;
	}

IL_01a0:
	{
		goto IL_01d7;
	}

IL_01d7:
	{
		goto IL_0340;
	}

IL_0211:
	{
		bool L_3 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_3)
		{
			goto IL_0243;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_4 = __this->___m_jclass;
		intptr_t L_5;
		L_5 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_4, NULL);
		intptr_t L_6 = ___0_fieldID;
		String_t* L_7;
		L_7 = AndroidJNISafe_GetStaticStringField_mB3D1325B08A38C7DAF1FA3E6CB52F6D8E0A2CB47(L_5, L_6, NULL);
		return ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_7, il2cpp_rgctx_data(method->rgctx_data, 1)));
	}

IL_0243:
	{
		bool L_8 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_8)
		{
			goto IL_0293;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_9 = __this->___m_jclass;
		intptr_t L_10;
		L_10 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_9, NULL);
		intptr_t L_11 = ___0_fieldID;
		intptr_t L_12;
		L_12 = AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867(L_10, L_11, NULL);
		V_0 = L_12;
		intptr_t L_13 = V_0;
		bool L_14;
		L_14 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_13, 0, NULL);
		if (L_14)
		{
			goto IL_0289;
		}
	}
	{
		intptr_t L_15 = V_0;
		AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_16;
		L_16 = AndroidJavaObject_AndroidJavaClassDeleteLocalRef_m56C84D7516BCB51A84E8AFDB3FCA46BAF494548F(L_15, NULL);
		return ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_16, il2cpp_rgctx_data(method->rgctx_data, 1)));
	}

IL_0289:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Il2CppSharedGenericObject*));
		Il2CppSharedGenericObject* L_17 = V_1;
		return L_17;
	}

IL_0293:
	{
		bool L_18 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_18)
		{
			goto IL_02e3;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_19 = __this->___m_jclass;
		intptr_t L_20;
		L_20 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_19, NULL);
		intptr_t L_21 = ___0_fieldID;
		intptr_t L_22;
		L_22 = AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867(L_20, L_21, NULL);
		V_2 = L_22;
		intptr_t L_23 = V_2;
		bool L_24;
		L_24 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_23, 0, NULL);
		if (L_24)
		{
			goto IL_02d9;
		}
	}
	{
		intptr_t L_25 = V_2;
		AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_26;
		L_26 = AndroidJavaObject_AndroidJavaObjectDeleteLocalRef_m2ECEEAF6389ABB9D6B963B8A98568ECD9413DF3C(L_25, NULL);
		return ((Il2CppSharedGenericObject*)Castclass((RuntimeObject*)L_26, il2cpp_rgctx_data(method->rgctx_data, 1)));
	}

IL_02d9:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Il2CppSharedGenericObject*));
		Il2CppSharedGenericObject* L_27 = V_1;
		return L_27;
	}

IL_02e3:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_28 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_29;
		L_29 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_28, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_30 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_31;
		L_31 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_30, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_32;
		L_32 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_29, L_31, NULL);
		if (!L_32)
		{
			goto IL_0315;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_33 = __this->___m_jclass;
		intptr_t L_34;
		L_34 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_33, NULL);
		intptr_t L_35 = ___0_fieldID;
		intptr_t L_36;
		L_36 = AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867(L_34, L_35, NULL);
		Il2CppSharedGenericObject* L_37;
		L_37 = AndroidJavaObject_FromJavaArrayDeleteLocalRef_TisIl2CppSharedGenericObject_mB1CEFE624E92B82D234C1982AA2A7DF6AC5FD284(L_36, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_37;
	}

IL_0315:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_38 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_39;
		L_39 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_38, NULL);
		Type_t* L_40 = L_39;
		if (L_40)
		{
			G_B33_0 = L_40;
			G_B33_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_032b;
		}
		G_B32_0 = L_40;
		G_B32_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B34_0 = ((String_t*)(NULL));
		G_B34_1 = G_B32_1;
		goto IL_0330;
	}

IL_032b:
	{
		NullCheck((RuntimeObject*)G_B33_0);
		String_t* L_41;
		L_41 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B33_0);
		G_B34_0 = L_41;
		G_B34_1 = G_B33_1;
	}

IL_0330:
	{
		String_t* L_42;
		L_42 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B34_1, G_B34_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_43 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_43, L_42, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_43, method);
	}

IL_0340:
	{
		il2cpp_codegen_initobj((&V_1), sizeof(Il2CppSharedGenericObject*));
		Il2CppSharedGenericObject* L_44 = V_1;
		return L_44;
	}
}
// Method Definition Index: 63080
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_m177F1A057E8F5EF05D9158BA272E890C6210044B_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = AndroidJNIHelper_GetFieldID_TisIl2CppSharedGenericObject_mDFF491584A820BE0629A47ED3B790A199050D589(L_1, L_2, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		Il2CppSharedGenericObject* L_5;
		L_5 = AndroidJavaObject__GetStatic_TisIl2CppSharedGenericObject_mF12B3A136CBDB98DF0876DF1FC47EA4A14D98A17(__this, L_4, il2cpp_rgctx_method(method->rgctx_data, 1));
		return L_5;
	}
}
// Method Definition Index: 63081
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__GetStatic_TisIl2CppFullySharedGenericAny_m265EB43F18C8FB22DCD571C5E50A76642369551D_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral50E02893F5B6939534A49C535241D6B7BAFCF599);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_9 = alloca(SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
	const Il2CppFullySharedGenericAny L_16 = L_9;
	const Il2CppFullySharedGenericAny L_24 = L_9;
	const Il2CppFullySharedGenericAny L_31 = L_9;
	const Il2CppFullySharedGenericAny L_38 = L_9;
	const Il2CppFullySharedGenericAny L_45 = L_9;
	const Il2CppFullySharedGenericAny L_52 = L_9;
	const Il2CppFullySharedGenericAny L_59 = L_9;
	const Il2CppFullySharedGenericAny L_66 = L_9;
	const Il2CppFullySharedGenericAny L_72 = L_9;
	const Il2CppFullySharedGenericAny L_83 = L_9;
	const Il2CppFullySharedGenericAny L_85 = L_9;
	const Il2CppFullySharedGenericAny L_95 = L_9;
	const Il2CppFullySharedGenericAny L_97 = L_9;
	const Il2CppFullySharedGenericAny L_107 = L_9;
	const Il2CppFullySharedGenericAny L_114 = L_9;
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	Il2CppFullySharedGenericAny V_1 = alloca(SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
	memset(V_1, 0, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
	intptr_t V_2;
	memset((&V_2), 0, sizeof(V_2));
	Type_t* G_B33_0 = NULL;
	String_t* G_B33_1 = NULL;
	Type_t* G_B32_0 = NULL;
	String_t* G_B32_1 = NULL;
	String_t* G_B34_0 = NULL;
	String_t* G_B34_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_0211;
		}
	}
	{
		bool L_3 = (il2cpp_defaults.int32_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_3)
		{
			goto IL_004b;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_4 = __this->___m_jclass;
		intptr_t L_5;
		L_5 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_4, NULL);
		intptr_t L_6 = ___0_fieldID;
		int32_t L_7;
		L_7 = AndroidJNISafe_GetStaticIntField_m0698D50C44E490A009E8388C7321630DED5973BD(L_5, L_6, NULL);
		int32_t L_8 = L_7;
		il2cpp_codegen_box_unbox((&L_8), L_9, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.int32_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_9, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_004b:
	{
		bool L_10 = (il2cpp_defaults.boolean_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_10)
		{
			goto IL_0082;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_11 = __this->___m_jclass;
		intptr_t L_12;
		L_12 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_11, NULL);
		intptr_t L_13 = ___0_fieldID;
		bool L_14;
		L_14 = AndroidJNISafe_GetStaticBooleanField_m172BEAA3F0AB6754EA5F1AD30C36DAA0D3D7C666(L_12, L_13, NULL);
		bool L_15 = L_14;
		il2cpp_codegen_box_unbox((&L_15), L_16, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.boolean_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_16, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0082:
	{
		bool L_17 = (il2cpp_defaults.byte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_17)
		{
			goto IL_00c4;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteral50E02893F5B6939534A49C535241D6B7BAFCF599, NULL);
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_18 = __this->___m_jclass;
		intptr_t L_19;
		L_19 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_18, NULL);
		intptr_t L_20 = ___0_fieldID;
		int8_t L_21;
		L_21 = AndroidJNISafe_GetStaticSByteField_m77596E5B1AE58DAFF39268AC954CAD53974A688D(L_19, L_20, NULL);
		uint8_t L_22 = (il2cpp_codegen_conv<uint8_t,int8_t,int32_t,false,false>(L_21,NULL));
		uint8_t L_23 = L_22;
		il2cpp_codegen_box_unbox((&L_23), L_24, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.byte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_24, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_00c4:
	{
		bool L_25 = (il2cpp_defaults.sbyte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_25)
		{
			goto IL_00fb;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_26 = __this->___m_jclass;
		intptr_t L_27;
		L_27 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_26, NULL);
		intptr_t L_28 = ___0_fieldID;
		int8_t L_29;
		L_29 = AndroidJNISafe_GetStaticSByteField_m77596E5B1AE58DAFF39268AC954CAD53974A688D(L_27, L_28, NULL);
		int8_t L_30 = L_29;
		il2cpp_codegen_box_unbox((&L_30), L_31, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.sbyte_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_31, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_00fb:
	{
		bool L_32 = (il2cpp_defaults.int16_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_32)
		{
			goto IL_0132;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_33 = __this->___m_jclass;
		intptr_t L_34;
		L_34 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_33, NULL);
		intptr_t L_35 = ___0_fieldID;
		int16_t L_36;
		L_36 = AndroidJNISafe_GetStaticShortField_m83716D4D85B30F26803F866AC47D5C04AAB5D320(L_34, L_35, NULL);
		int16_t L_37 = L_36;
		il2cpp_codegen_box_unbox((&L_37), L_38, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.int16_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_38, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0132:
	{
		bool L_39 = (il2cpp_defaults.int64_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_39)
		{
			goto IL_0169;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_40 = __this->___m_jclass;
		intptr_t L_41;
		L_41 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_40, NULL);
		intptr_t L_42 = ___0_fieldID;
		int64_t L_43;
		L_43 = AndroidJNISafe_GetStaticLongField_mABC2B933CEB757E3FAF1FD6C60AA0C4D38E9C49D(L_41, L_42, NULL);
		int64_t L_44 = L_43;
		il2cpp_codegen_box_unbox((&L_44), L_45, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.int64_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_45, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0169:
	{
		bool L_46 = (il2cpp_defaults.single_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_46)
		{
			goto IL_01a0;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jclass;
		intptr_t L_48;
		L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
		intptr_t L_49 = ___0_fieldID;
		float L_50;
		L_50 = AndroidJNISafe_GetStaticFloatField_mD1456B729026959309A839C2647279C0B6541356(L_48, L_49, NULL);
		float L_51 = L_50;
		il2cpp_codegen_box_unbox((&L_51), L_52, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.single_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_52, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_01a0:
	{
		bool L_53 = (il2cpp_defaults.double_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_53)
		{
			goto IL_01d7;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_54 = __this->___m_jclass;
		intptr_t L_55;
		L_55 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_54, NULL);
		intptr_t L_56 = ___0_fieldID;
		double L_57;
		L_57 = AndroidJNISafe_GetStaticDoubleField_mEB86F2CE1F3879AAA9DEDA4B496F882C0E1DCBC2(L_55, L_56, NULL);
		double L_58 = L_57;
		il2cpp_codegen_box_unbox((&L_58), L_59, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.double_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_59, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_01d7:
	{
		bool L_60 = (il2cpp_defaults.char_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_60)
		{
			goto IL_0340;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_61 = __this->___m_jclass;
		intptr_t L_62;
		L_62 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_61, NULL);
		intptr_t L_63 = ___0_fieldID;
		Il2CppChar L_64;
		L_64 = AndroidJNISafe_GetStaticCharField_mF70F6D197261364AF2A9E875D84DDDA35BD0ED96(L_62, L_63, NULL);
		Il2CppChar L_65 = L_64;
		il2cpp_codegen_box_unbox((&L_65), L_66, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3, il2cpp_defaults.char_class, il2cpp_rgctx_data(method->rgctx_data, 1));
		il2cpp_codegen_memcpy(il2cppRetVal, L_66, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0211:
	{
		bool L_67 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_67)
		{
			goto IL_0243;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_68 = __this->___m_jclass;
		intptr_t L_69;
		L_69 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_68, NULL);
		intptr_t L_70 = ___0_fieldID;
		String_t* L_71;
		L_71 = AndroidJNISafe_GetStaticStringField_mB3D1325B08A38C7DAF1FA3E6CB52F6D8E0A2CB47(L_69, L_70, NULL);
		void* L_73 = UnBox_Any((RuntimeObject*)L_71, il2cpp_rgctx_data(method->rgctx_data, 1), L_72);
		il2cpp_codegen_memcpy(il2cppRetVal, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_73)), SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0243:
	{
		bool L_74 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_74)
		{
			goto IL_0293;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jclass;
		intptr_t L_76;
		L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
		intptr_t L_77 = ___0_fieldID;
		intptr_t L_78;
		L_78 = AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867(L_76, L_77, NULL);
		V_0 = L_78;
		intptr_t L_79 = V_0;
		bool L_80;
		L_80 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_79, 0, NULL);
		if (L_80)
		{
			goto IL_0289;
		}
	}
	{
		intptr_t L_81 = V_0;
		AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03* L_82;
		L_82 = AndroidJavaObject_AndroidJavaClassDeleteLocalRef_m56C84D7516BCB51A84E8AFDB3FCA46BAF494548F(L_81, NULL);
		void* L_84 = UnBox_Any((RuntimeObject*)L_82, il2cpp_rgctx_data(method->rgctx_data, 1), L_83);
		il2cpp_codegen_memcpy(il2cppRetVal, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_84)), SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0289:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_1, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		il2cpp_codegen_memcpy(L_85, V_1, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		il2cpp_codegen_memcpy(il2cppRetVal, L_85, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0293:
	{
		bool L_86 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_86)
		{
			goto IL_02e3;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_87 = __this->___m_jclass;
		intptr_t L_88;
		L_88 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_87, NULL);
		intptr_t L_89 = ___0_fieldID;
		intptr_t L_90;
		L_90 = AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867(L_88, L_89, NULL);
		V_2 = L_90;
		intptr_t L_91 = V_2;
		bool L_92;
		L_92 = IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline(L_91, 0, NULL);
		if (L_92)
		{
			goto IL_02d9;
		}
	}
	{
		intptr_t L_93 = V_2;
		AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* L_94;
		L_94 = AndroidJavaObject_AndroidJavaObjectDeleteLocalRef_m2ECEEAF6389ABB9D6B963B8A98568ECD9413DF3C(L_93, NULL);
		void* L_96 = UnBox_Any((RuntimeObject*)L_94, il2cpp_rgctx_data(method->rgctx_data, 1), L_95);
		il2cpp_codegen_memcpy(il2cppRetVal, (((Il2CppFullySharedGenericAny)(Il2CppFullySharedGenericAny*)L_96)), SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_02d9:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_1, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		il2cpp_codegen_memcpy(L_97, V_1, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		il2cpp_codegen_memcpy(il2cppRetVal, L_97, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_02e3:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_98 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_99;
		L_99 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_98, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_100 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_101;
		L_101 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_100, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_102;
		L_102 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_99, L_101, NULL);
		if (!L_102)
		{
			goto IL_0315;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_103 = __this->___m_jclass;
		intptr_t L_104;
		L_104 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_103, NULL);
		intptr_t L_105 = ___0_fieldID;
		intptr_t L_106;
		L_106 = AndroidJNISafe_GetStaticObjectField_mB6B9A9EB2619DFDF1DA56300BF9FEC19BF883867(L_104, L_105, NULL);
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_106, (Il2CppFullySharedGenericAny*)L_107);
		il2cpp_codegen_memcpy(il2cppRetVal, L_107, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}

IL_0315:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_108 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_109;
		L_109 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_108, NULL);
		Type_t* L_110 = L_109;
		if (L_110)
		{
			G_B33_0 = L_110;
			G_B33_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_032b;
		}
		G_B32_0 = L_110;
		G_B32_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B34_0 = ((String_t*)(NULL));
		G_B34_1 = G_B32_1;
		goto IL_0330;
	}

IL_032b:
	{
		NullCheck((RuntimeObject*)G_B33_0);
		String_t* L_111;
		L_111 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B33_0);
		G_B34_0 = L_111;
		G_B34_1 = G_B33_1;
	}

IL_0330:
	{
		String_t* L_112;
		L_112 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B34_1, G_B34_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_113 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_113, L_112, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_113, method);
	}

IL_0340:
	{
		il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_1, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		il2cpp_codegen_memcpy(L_114, V_1, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		il2cpp_codegen_memcpy(il2cppRetVal, L_114, SizeOf_FieldType_t4130D50F8DF6CBFB2A17DDF9CA62DF84750A5FF3);
		return;
	}
}
// Method Definition Index: 63080
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__GetStatic_TisIl2CppFullySharedGenericAny_m5FA6F86B221B6AD76DDD71818F721187D59BFC38_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_t31FF3BC94504224433BABB9DAD6CE4032EA80634 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_FieldType_t31FF3BC94504224433BABB9DAD6CE4032EA80634);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_1, L_2, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), __this, L_4, (Il2CppFullySharedGenericAny*)L_5);
		il2cpp_codegen_memcpy(il2cppRetVal, L_5, SizeOf_FieldType_t31FF3BC94504224433BABB9DAD6CE4032EA80634);
		return;
	}
}
// Method Definition Index: 63075
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Set_TisIl2CppFullySharedGenericAny_m84E47B5A5FF416288BF85C7F79F287D6FD90356F_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral1D86DE1FC0445C55F712ED0F6D3E32FED0A1E724);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_7 = alloca(SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
	const Il2CppFullySharedGenericAny L_13 = L_7;
	const Il2CppFullySharedGenericAny L_19 = L_7;
	const Il2CppFullySharedGenericAny L_26 = L_7;
	const Il2CppFullySharedGenericAny L_32 = L_7;
	const Il2CppFullySharedGenericAny L_38 = L_7;
	const Il2CppFullySharedGenericAny L_44 = L_7;
	const Il2CppFullySharedGenericAny L_50 = L_7;
	const Il2CppFullySharedGenericAny L_56 = L_7;
	const Il2CppFullySharedGenericAny L_62 = L_7;
	const Il2CppFullySharedGenericAny L_68 = L_7;
	const Il2CppFullySharedGenericAny L_70 = L_7;
	const Il2CppFullySharedGenericAny L_78 = L_7;
	const Il2CppFullySharedGenericAny L_80 = L_7;
	const Il2CppFullySharedGenericAny L_92 = L_7;
	const Il2CppFullySharedGenericAny L_94 = L_7;
	const Il2CppFullySharedGenericAny L_102 = L_7;
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	intptr_t G_B24_0;
	memset((&G_B24_0), 0, sizeof(G_B24_0));
	intptr_t G_B24_1;
	memset((&G_B24_1), 0, sizeof(G_B24_1));
	intptr_t G_B23_0;
	memset((&G_B23_0), 0, sizeof(G_B23_0));
	intptr_t G_B23_1;
	memset((&G_B23_1), 0, sizeof(G_B23_1));
	intptr_t G_B25_0;
	memset((&G_B25_0), 0, sizeof(G_B25_0));
	intptr_t G_B25_1;
	memset((&G_B25_1), 0, sizeof(G_B25_1));
	intptr_t G_B25_2;
	memset((&G_B25_2), 0, sizeof(G_B25_2));
	intptr_t G_B29_0;
	memset((&G_B29_0), 0, sizeof(G_B29_0));
	intptr_t G_B29_1;
	memset((&G_B29_1), 0, sizeof(G_B29_1));
	intptr_t G_B28_0;
	memset((&G_B28_0), 0, sizeof(G_B28_0));
	intptr_t G_B28_1;
	memset((&G_B28_1), 0, sizeof(G_B28_1));
	intptr_t G_B30_0;
	memset((&G_B30_0), 0, sizeof(G_B30_0));
	intptr_t G_B30_1;
	memset((&G_B30_1), 0, sizeof(G_B30_1));
	intptr_t G_B30_2;
	memset((&G_B30_2), 0, sizeof(G_B30_2));
	intptr_t G_B34_0;
	memset((&G_B34_0), 0, sizeof(G_B34_0));
	intptr_t G_B34_1;
	memset((&G_B34_1), 0, sizeof(G_B34_1));
	intptr_t G_B33_0;
	memset((&G_B33_0), 0, sizeof(G_B33_0));
	intptr_t G_B33_1;
	memset((&G_B33_1), 0, sizeof(G_B33_1));
	intptr_t G_B35_0;
	memset((&G_B35_0), 0, sizeof(G_B35_0));
	intptr_t G_B35_1;
	memset((&G_B35_1), 0, sizeof(G_B35_1));
	intptr_t G_B35_2;
	memset((&G_B35_2), 0, sizeof(G_B35_2));
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_021a;
		}
	}
	{
		bool L_3 = (il2cpp_defaults.int32_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_3)
		{
			goto IL_004c;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_4 = __this->___m_jobject;
		intptr_t L_5;
		L_5 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_4, NULL);
		intptr_t L_6 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_7, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		int32_t L_8;
		il2cpp_codegen_box_unbox(L_7, (&L_8), sizeof(int32_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.int32_class);
		AndroidJNISafe_SetIntField_mD238DA37BA1B3D7693484237951A6EFEA9C62120(L_5, L_6, L_8, NULL);
		return;
	}

IL_004c:
	{
		bool L_9 = (il2cpp_defaults.boolean_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_9)
		{
			goto IL_0084;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_10 = __this->___m_jobject;
		intptr_t L_11;
		L_11 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_10, NULL);
		intptr_t L_12 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_13, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		bool L_14;
		il2cpp_codegen_box_unbox(L_13, (&L_14), sizeof(bool), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.boolean_class);
		AndroidJNISafe_SetBooleanField_m5279EA41B214699E79733DC6C93259CC9DCA1D9E(L_11, L_12, L_14, NULL);
		return;
	}

IL_0084:
	{
		bool L_15 = (il2cpp_defaults.byte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_15)
		{
			goto IL_00c7;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteral1D86DE1FC0445C55F712ED0F6D3E32FED0A1E724, NULL);
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_16 = __this->___m_jobject;
		intptr_t L_17;
		L_17 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_16, NULL);
		intptr_t L_18 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_19, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		uint8_t L_20;
		il2cpp_codegen_box_unbox(L_19, (&L_20), sizeof(uint8_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.byte_class);
		int8_t L_21 = (il2cpp_codegen_conv<int8_t,uint8_t,int32_t,false,false>(L_20,NULL));
		AndroidJNISafe_SetSByteField_mB021168746571E7CAA8C0EAD7AA7F02C18B5EE33(L_17, L_18, L_21, NULL);
		return;
	}

IL_00c7:
	{
		bool L_22 = (il2cpp_defaults.sbyte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_22)
		{
			goto IL_00ff;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_23 = __this->___m_jobject;
		intptr_t L_24;
		L_24 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_23, NULL);
		intptr_t L_25 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_26, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		int8_t L_27;
		il2cpp_codegen_box_unbox(L_26, (&L_27), sizeof(int8_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.sbyte_class);
		AndroidJNISafe_SetSByteField_mB021168746571E7CAA8C0EAD7AA7F02C18B5EE33(L_24, L_25, L_27, NULL);
		return;
	}

IL_00ff:
	{
		bool L_28 = (il2cpp_defaults.int16_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_28)
		{
			goto IL_0137;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_29 = __this->___m_jobject;
		intptr_t L_30;
		L_30 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_29, NULL);
		intptr_t L_31 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_32, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		int16_t L_33;
		il2cpp_codegen_box_unbox(L_32, (&L_33), sizeof(int16_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.int16_class);
		AndroidJNISafe_SetShortField_mF95E569C142DEDD604CE8BA7617328B3EDDD2F0D(L_30, L_31, L_33, NULL);
		return;
	}

IL_0137:
	{
		bool L_34 = (il2cpp_defaults.int64_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_34)
		{
			goto IL_016f;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_35 = __this->___m_jobject;
		intptr_t L_36;
		L_36 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_35, NULL);
		intptr_t L_37 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_38, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		int64_t L_39;
		il2cpp_codegen_box_unbox(L_38, (&L_39), sizeof(int64_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.int64_class);
		AndroidJNISafe_SetLongField_m13905547F5CDC7E01AB0D8C787BF98DC2870EC35(L_36, L_37, L_39, NULL);
		return;
	}

IL_016f:
	{
		bool L_40 = (il2cpp_defaults.single_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_40)
		{
			goto IL_01a7;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_41 = __this->___m_jobject;
		intptr_t L_42;
		L_42 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_41, NULL);
		intptr_t L_43 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_44, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		float L_45;
		il2cpp_codegen_box_unbox(L_44, (&L_45), sizeof(float), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.single_class);
		AndroidJNISafe_SetFloatField_m589CA6B8DD2BFD4515C5AEAE3772782B293F02C3(L_42, L_43, L_45, NULL);
		return;
	}

IL_01a7:
	{
		bool L_46 = (il2cpp_defaults.double_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_46)
		{
			goto IL_01df;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jobject;
		intptr_t L_48;
		L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
		intptr_t L_49 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_50, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		double L_51;
		il2cpp_codegen_box_unbox(L_50, (&L_51), sizeof(double), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.double_class);
		AndroidJNISafe_SetDoubleField_mE93D0C5EC2019A1B657BD32970FE6EFC9B005A58(L_48, L_49, L_51, NULL);
		return;
	}

IL_01df:
	{
		bool L_52 = (il2cpp_defaults.char_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_52)
		{
			goto IL_03aa;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_53 = __this->___m_jobject;
		intptr_t L_54;
		L_54 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_53, NULL);
		intptr_t L_55 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_56, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		Il2CppChar L_57;
		il2cpp_codegen_box_unbox(L_56, (&L_57), sizeof(Il2CppChar), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.char_class);
		AndroidJNISafe_SetCharField_m69D09A6A2CEA55D84B240FE32D90300AAB1334F9(L_54, L_55, L_57, NULL);
		return;
	}

IL_021a:
	{
		bool L_58 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_58)
		{
			goto IL_0252;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_59 = __this->___m_jobject;
		intptr_t L_60;
		L_60 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_59, NULL);
		intptr_t L_61 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_62, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		RuntimeObject* L_63 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_62);
		AndroidJNISafe_SetStringField_m649363D4E87763D6A9760359EAFB29802E90B409(L_60, L_61, ((String_t*)CastclassSealed((RuntimeObject*)L_63, il2cpp_defaults.string_class)), NULL);
		return;
	}

IL_0252:
	{
		bool L_64 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_64)
		{
			goto IL_02a3;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_65 = __this->___m_jobject;
		intptr_t L_66;
		L_66 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_65, NULL);
		intptr_t L_67 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_68, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		bool L_69 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_68);
		if (!L_69)
		{
			G_B24_0 = L_67;
			G_B24_1 = L_66;
			goto IL_0298;
		}
		G_B23_0 = L_67;
		G_B23_1 = L_66;
	}
	{
		il2cpp_codegen_memcpy(L_70, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		RuntimeObject* L_71 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_70);
		NullCheck(((AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)CastclassClass((RuntimeObject*)L_71, AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var)));
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_72 = ((AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)((AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)CastclassClass((RuntimeObject*)L_71, AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var)))->___m_jclass;
		intptr_t L_73;
		L_73 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_72, NULL);
		G_B25_0 = L_73;
		G_B25_1 = G_B23_0;
		G_B25_2 = G_B23_1;
		goto IL_029d;
	}

IL_0298:
	{
		G_B25_0 = 0;
		G_B25_1 = G_B24_0;
		G_B25_2 = G_B24_1;
	}

IL_029d:
	{
		AndroidJNISafe_SetObjectField_mFE500926F9C963FF106E8AA30A16F4C671BAA8CA(G_B25_2, G_B25_1, G_B25_0, NULL);
		return;
	}

IL_02a3:
	{
		bool L_74 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_74)
		{
			goto IL_02f4;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jobject;
		intptr_t L_76;
		L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
		intptr_t L_77 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_78, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		bool L_79 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_78);
		if (!L_79)
		{
			G_B29_0 = L_77;
			G_B29_1 = L_76;
			goto IL_02e9;
		}
		G_B28_0 = L_77;
		G_B28_1 = L_76;
	}
	{
		il2cpp_codegen_memcpy(L_80, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		RuntimeObject* L_81 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_80);
		NullCheck(((AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)CastclassClass((RuntimeObject*)L_81, AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var)));
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = ((AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)CastclassClass((RuntimeObject*)L_81, AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var))->___m_jobject;
		intptr_t L_83;
		L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
		G_B30_0 = L_83;
		G_B30_1 = G_B28_0;
		G_B30_2 = G_B28_1;
		goto IL_02ee;
	}

IL_02e9:
	{
		G_B30_0 = 0;
		G_B30_1 = G_B29_0;
		G_B30_2 = G_B29_1;
	}

IL_02ee:
	{
		AndroidJNISafe_SetObjectField_mFE500926F9C963FF106E8AA30A16F4C671BAA8CA(G_B30_2, G_B30_1, G_B30_0, NULL);
		return;
	}

IL_02f4:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_84 = { reinterpret_cast<intptr_t> (AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_85;
		L_85 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_84, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_86 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_87;
		L_87 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_86, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_88;
		L_88 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_85, L_87, NULL);
		if (!L_88)
		{
			goto IL_0340;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_89 = __this->___m_jobject;
		intptr_t L_90;
		L_90 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_89, NULL);
		intptr_t L_91 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_92, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		bool L_93 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_92);
		if (!L_93)
		{
			G_B34_0 = L_91;
			G_B34_1 = L_90;
			goto IL_0335;
		}
		G_B33_0 = L_91;
		G_B33_1 = L_90;
	}
	{
		il2cpp_codegen_memcpy(L_94, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		RuntimeObject* L_95 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_94);
		NullCheck(((AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D*)CastclassClass((RuntimeObject*)L_95, AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_il2cpp_TypeInfo_var)));
		intptr_t L_96;
		L_96 = AndroidJavaProxy_GetRawProxy_m685E066A4D378B596CD88385B954AE90CBF328A9(((AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D*)CastclassClass((RuntimeObject*)L_95, AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_il2cpp_TypeInfo_var)), NULL);
		G_B35_0 = L_96;
		G_B35_1 = G_B33_0;
		G_B35_2 = G_B33_1;
		goto IL_033a;
	}

IL_0335:
	{
		G_B35_0 = 0;
		G_B35_1 = G_B34_0;
		G_B35_2 = G_B34_1;
	}

IL_033a:
	{
		AndroidJNISafe_SetObjectField_mFE500926F9C963FF106E8AA30A16F4C671BAA8CA(G_B35_2, G_B35_1, G_B35_0, NULL);
		return;
	}

IL_0340:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_97 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_98;
		L_98 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_97, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_99 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_100;
		L_100 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_99, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_101;
		L_101 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_98, L_100, NULL);
		if (!L_101)
		{
			goto IL_037f;
		}
	}
	{
		il2cpp_codegen_memcpy(L_102, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tC72607BAF758371FA7A7DDA2D0873E433EE11D19);
		RuntimeObject* L_103 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_102);
		intptr_t L_104;
		L_104 = AndroidJNIHelper_ConvertToJNIArray_mBEAE4605FF297D19AFB8CE4E8443C9C0F87E9A13(((RuntimeArray*)CastclassClass((RuntimeObject*)L_103, il2cpp_defaults.array_class)), NULL);
		V_0 = L_104;
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_105 = __this->___m_jobject;
		intptr_t L_106;
		L_106 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_105, NULL);
		intptr_t L_107 = ___0_fieldID;
		intptr_t L_108 = V_0;
		AndroidJNISafe_SetObjectField_mFE500926F9C963FF106E8AA30A16F4C671BAA8CA(L_106, L_107, L_108, NULL);
		return;
	}

IL_037f:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_110;
		L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
		Type_t* L_111 = L_110;
		if (L_111)
		{
			G_B40_0 = L_111;
			G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_0395;
		}
		G_B39_0 = L_111;
		G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B41_0 = ((String_t*)(NULL));
		G_B41_1 = G_B39_1;
		goto IL_039a;
	}

IL_0395:
	{
		NullCheck((RuntimeObject*)G_B40_0);
		String_t* L_112;
		L_112 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
		G_B41_0 = L_112;
		G_B41_1 = G_B40_1;
	}

IL_039a:
	{
		String_t* L_113;
		L_113 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_114 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_114, L_113, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_114, method);
	}

IL_03aa:
	{
		return;
	}
}
// Method Definition Index: 63074
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__Set_TisIl2CppFullySharedGenericAny_m1AF6344C2EF172B8799D007F4BAC3ECDC182BC22_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_t2F905C7C598CE04C35E1297452F5AE8BFD208DAE = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_FieldType_t2F905C7C598CE04C35E1297452F5AE8BFD208DAE);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_1, L_2, (bool)0, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		il2cpp_codegen_memcpy(L_5, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_t2F905C7C598CE04C35E1297452F5AE8BFD208DAE);
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), __this, L_4, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_5: *(void**)L_5));
		return;
	}
}
// Method Definition Index: 63083
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__SetStatic_TisIl2CppFullySharedGenericAny_m07564225CF639F8502927F2D783809867A34367A_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, intptr_t ___0_fieldID, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_0_0_0_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&_stringLiteral1D86DE1FC0445C55F712ED0F6D3E32FED0A1E724);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_7 = alloca(SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
	const Il2CppFullySharedGenericAny L_13 = L_7;
	const Il2CppFullySharedGenericAny L_19 = L_7;
	const Il2CppFullySharedGenericAny L_26 = L_7;
	const Il2CppFullySharedGenericAny L_32 = L_7;
	const Il2CppFullySharedGenericAny L_38 = L_7;
	const Il2CppFullySharedGenericAny L_44 = L_7;
	const Il2CppFullySharedGenericAny L_50 = L_7;
	const Il2CppFullySharedGenericAny L_56 = L_7;
	const Il2CppFullySharedGenericAny L_62 = L_7;
	const Il2CppFullySharedGenericAny L_68 = L_7;
	const Il2CppFullySharedGenericAny L_70 = L_7;
	const Il2CppFullySharedGenericAny L_78 = L_7;
	const Il2CppFullySharedGenericAny L_80 = L_7;
	const Il2CppFullySharedGenericAny L_92 = L_7;
	const Il2CppFullySharedGenericAny L_94 = L_7;
	const Il2CppFullySharedGenericAny L_102 = L_7;
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	intptr_t G_B24_0;
	memset((&G_B24_0), 0, sizeof(G_B24_0));
	intptr_t G_B24_1;
	memset((&G_B24_1), 0, sizeof(G_B24_1));
	intptr_t G_B23_0;
	memset((&G_B23_0), 0, sizeof(G_B23_0));
	intptr_t G_B23_1;
	memset((&G_B23_1), 0, sizeof(G_B23_1));
	intptr_t G_B25_0;
	memset((&G_B25_0), 0, sizeof(G_B25_0));
	intptr_t G_B25_1;
	memset((&G_B25_1), 0, sizeof(G_B25_1));
	intptr_t G_B25_2;
	memset((&G_B25_2), 0, sizeof(G_B25_2));
	intptr_t G_B29_0;
	memset((&G_B29_0), 0, sizeof(G_B29_0));
	intptr_t G_B29_1;
	memset((&G_B29_1), 0, sizeof(G_B29_1));
	intptr_t G_B28_0;
	memset((&G_B28_0), 0, sizeof(G_B28_0));
	intptr_t G_B28_1;
	memset((&G_B28_1), 0, sizeof(G_B28_1));
	intptr_t G_B30_0;
	memset((&G_B30_0), 0, sizeof(G_B30_0));
	intptr_t G_B30_1;
	memset((&G_B30_1), 0, sizeof(G_B30_1));
	intptr_t G_B30_2;
	memset((&G_B30_2), 0, sizeof(G_B30_2));
	intptr_t G_B34_0;
	memset((&G_B34_0), 0, sizeof(G_B34_0));
	intptr_t G_B34_1;
	memset((&G_B34_1), 0, sizeof(G_B34_1));
	intptr_t G_B33_0;
	memset((&G_B33_0), 0, sizeof(G_B33_0));
	intptr_t G_B33_1;
	memset((&G_B33_1), 0, sizeof(G_B33_1));
	intptr_t G_B35_0;
	memset((&G_B35_0), 0, sizeof(G_B35_0));
	intptr_t G_B35_1;
	memset((&G_B35_1), 0, sizeof(G_B35_1));
	intptr_t G_B35_2;
	memset((&G_B35_2), 0, sizeof(G_B35_2));
	Type_t* G_B40_0 = NULL;
	String_t* G_B40_1 = NULL;
	Type_t* G_B39_0 = NULL;
	String_t* G_B39_1 = NULL;
	String_t* G_B41_0 = NULL;
	String_t* G_B41_1 = NULL;
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_0 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_1;
		L_1 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_0, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_2;
		L_2 = AndroidReflection_IsPrimitive_mA41A9ECECE3D73679C79DC8B0FDD32B59570DF25(L_1, NULL);
		if (!L_2)
		{
			goto IL_021a;
		}
	}
	{
		bool L_3 = (il2cpp_defaults.int32_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_3)
		{
			goto IL_004c;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_4 = __this->___m_jclass;
		intptr_t L_5;
		L_5 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_4, NULL);
		intptr_t L_6 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_7, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		int32_t L_8;
		il2cpp_codegen_box_unbox(L_7, (&L_8), sizeof(int32_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.int32_class);
		AndroidJNISafe_SetStaticIntField_m1E20F6C72260CAFBF73207DCEC1816B2816EEBE1(L_5, L_6, L_8, NULL);
		return;
	}

IL_004c:
	{
		bool L_9 = (il2cpp_defaults.boolean_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_9)
		{
			goto IL_0084;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_10 = __this->___m_jclass;
		intptr_t L_11;
		L_11 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_10, NULL);
		intptr_t L_12 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_13, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		bool L_14;
		il2cpp_codegen_box_unbox(L_13, (&L_14), sizeof(bool), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.boolean_class);
		AndroidJNISafe_SetStaticBooleanField_mBE4E40DA1B07A29D356AEEE6CB9519F2B3621AC9(L_11, L_12, L_14, NULL);
		return;
	}

IL_0084:
	{
		bool L_15 = (il2cpp_defaults.byte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_15)
		{
			goto IL_00c7;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(Debug_t8394C7EEAECA3689C2C9B9DE9C7166D73596276F_il2cpp_TypeInfo_var);
		Debug_LogWarning_m33EF1B897E0C7C6FF538989610BFAFFEF4628CA9((RuntimeObject*)_stringLiteral1D86DE1FC0445C55F712ED0F6D3E32FED0A1E724, NULL);
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_16 = __this->___m_jclass;
		intptr_t L_17;
		L_17 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_16, NULL);
		intptr_t L_18 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_19, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		uint8_t L_20;
		il2cpp_codegen_box_unbox(L_19, (&L_20), sizeof(uint8_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.byte_class);
		int8_t L_21 = (il2cpp_codegen_conv<int8_t,uint8_t,int32_t,false,false>(L_20,NULL));
		AndroidJNISafe_SetStaticSByteField_m242120982A9227E1E8344FFE9F06FD74986D15E9(L_17, L_18, L_21, NULL);
		return;
	}

IL_00c7:
	{
		bool L_22 = (il2cpp_defaults.sbyte_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_22)
		{
			goto IL_00ff;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_23 = __this->___m_jclass;
		intptr_t L_24;
		L_24 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_23, NULL);
		intptr_t L_25 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_26, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		int8_t L_27;
		il2cpp_codegen_box_unbox(L_26, (&L_27), sizeof(int8_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.sbyte_class);
		AndroidJNISafe_SetStaticSByteField_m242120982A9227E1E8344FFE9F06FD74986D15E9(L_24, L_25, L_27, NULL);
		return;
	}

IL_00ff:
	{
		bool L_28 = (il2cpp_defaults.int16_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_28)
		{
			goto IL_0137;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_29 = __this->___m_jclass;
		intptr_t L_30;
		L_30 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_29, NULL);
		intptr_t L_31 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_32, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		int16_t L_33;
		il2cpp_codegen_box_unbox(L_32, (&L_33), sizeof(int16_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.int16_class);
		AndroidJNISafe_SetStaticShortField_m92534AAA86D7E1055E12936C8A7BD6B865B7DB81(L_30, L_31, L_33, NULL);
		return;
	}

IL_0137:
	{
		bool L_34 = (il2cpp_defaults.int64_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_34)
		{
			goto IL_016f;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_35 = __this->___m_jclass;
		intptr_t L_36;
		L_36 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_35, NULL);
		intptr_t L_37 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_38, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		int64_t L_39;
		il2cpp_codegen_box_unbox(L_38, (&L_39), sizeof(int64_t), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.int64_class);
		AndroidJNISafe_SetStaticLongField_m299AAC2DE8B6747B0B5E109BABB2F3A4FC1F486E(L_36, L_37, L_39, NULL);
		return;
	}

IL_016f:
	{
		bool L_40 = (il2cpp_defaults.single_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_40)
		{
			goto IL_01a7;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_41 = __this->___m_jclass;
		intptr_t L_42;
		L_42 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_41, NULL);
		intptr_t L_43 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_44, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		float L_45;
		il2cpp_codegen_box_unbox(L_44, (&L_45), sizeof(float), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.single_class);
		AndroidJNISafe_SetStaticFloatField_mB2EDDE632AB2088CD12F1FD12174FB86990BCBEE(L_42, L_43, L_45, NULL);
		return;
	}

IL_01a7:
	{
		bool L_46 = (il2cpp_defaults.double_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_46)
		{
			goto IL_01df;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_47 = __this->___m_jclass;
		intptr_t L_48;
		L_48 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_47, NULL);
		intptr_t L_49 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_50, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		double L_51;
		il2cpp_codegen_box_unbox(L_50, (&L_51), sizeof(double), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.double_class);
		AndroidJNISafe_SetStaticDoubleField_mA0253927D476917D2158A9CE29F1BF535485B956(L_48, L_49, L_51, NULL);
		return;
	}

IL_01df:
	{
		bool L_52 = (il2cpp_defaults.char_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_52)
		{
			goto IL_03aa;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_53 = __this->___m_jclass;
		intptr_t L_54;
		L_54 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_53, NULL);
		intptr_t L_55 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_56, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		Il2CppChar L_57;
		il2cpp_codegen_box_unbox(L_56, (&L_57), sizeof(Il2CppChar), il2cpp_rgctx_data(method->rgctx_data, 1), il2cpp_defaults.char_class);
		AndroidJNISafe_SetStaticCharField_m2B8245275C36525798C869B7B1088B25BA663613(L_54, L_55, L_57, NULL);
		return;
	}

IL_021a:
	{
		bool L_58 = (il2cpp_defaults.string_class) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_58)
		{
			goto IL_0252;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_59 = __this->___m_jclass;
		intptr_t L_60;
		L_60 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_59, NULL);
		intptr_t L_61 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_62, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		RuntimeObject* L_63 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_62);
		AndroidJNISafe_SetStaticStringField_m445D977B2374056C6E4607FAEDB7E99A1353E2EE(L_60, L_61, ((String_t*)CastclassSealed((RuntimeObject*)L_63, il2cpp_defaults.string_class)), NULL);
		return;
	}

IL_0252:
	{
		bool L_64 = (AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_64)
		{
			goto IL_02a3;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_65 = __this->___m_jclass;
		intptr_t L_66;
		L_66 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_65, NULL);
		intptr_t L_67 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_68, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		bool L_69 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_68);
		if (!L_69)
		{
			G_B24_0 = L_67;
			G_B24_1 = L_66;
			goto IL_0298;
		}
		G_B23_0 = L_67;
		G_B23_1 = L_66;
	}
	{
		il2cpp_codegen_memcpy(L_70, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		RuntimeObject* L_71 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_70);
		NullCheck(((AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)CastclassClass((RuntimeObject*)L_71, AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var)));
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_72 = ((AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)((AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03*)CastclassClass((RuntimeObject*)L_71, AndroidJavaClass_tE6296B30CC4BF84434A9B765267F3FD0DD8DDB03_il2cpp_TypeInfo_var)))->___m_jclass;
		intptr_t L_73;
		L_73 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_72, NULL);
		G_B25_0 = L_73;
		G_B25_1 = G_B23_0;
		G_B25_2 = G_B23_1;
		goto IL_029d;
	}

IL_0298:
	{
		G_B25_0 = 0;
		G_B25_1 = G_B24_0;
		G_B25_2 = G_B24_1;
	}

IL_029d:
	{
		AndroidJNISafe_SetStaticObjectField_m7757F7E30F8122DAF89F138A8AE727CB896BC721(G_B25_2, G_B25_1, G_B25_0, NULL);
		return;
	}

IL_02a3:
	{
		bool L_74 = (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var) == (il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		if (!L_74)
		{
			goto IL_02f4;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_75 = __this->___m_jclass;
		intptr_t L_76;
		L_76 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_75, NULL);
		intptr_t L_77 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_78, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		bool L_79 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_78);
		if (!L_79)
		{
			G_B29_0 = L_77;
			G_B29_1 = L_76;
			goto IL_02e9;
		}
		G_B28_0 = L_77;
		G_B28_1 = L_76;
	}
	{
		il2cpp_codegen_memcpy(L_80, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		RuntimeObject* L_81 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_80);
		NullCheck(((AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)CastclassClass((RuntimeObject*)L_81, AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var)));
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_82 = ((AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0*)CastclassClass((RuntimeObject*)L_81, AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0_il2cpp_TypeInfo_var))->___m_jobject;
		intptr_t L_83;
		L_83 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_82, NULL);
		G_B30_0 = L_83;
		G_B30_1 = G_B28_0;
		G_B30_2 = G_B28_1;
		goto IL_02ee;
	}

IL_02e9:
	{
		G_B30_0 = 0;
		G_B30_1 = G_B29_0;
		G_B30_2 = G_B29_1;
	}

IL_02ee:
	{
		AndroidJNISafe_SetStaticObjectField_m7757F7E30F8122DAF89F138A8AE727CB896BC721(G_B30_2, G_B30_1, G_B30_0, NULL);
		return;
	}

IL_02f4:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_84 = { reinterpret_cast<intptr_t> (AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_0_0_0_var) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_85;
		L_85 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_84, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_86 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_87;
		L_87 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_86, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_88;
		L_88 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_85, L_87, NULL);
		if (!L_88)
		{
			goto IL_0340;
		}
	}
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_89 = __this->___m_jclass;
		intptr_t L_90;
		L_90 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_89, NULL);
		intptr_t L_91 = ___0_fieldID;
		il2cpp_codegen_memcpy(L_92, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		bool L_93 = il2cpp_codegen_would_box_to_non_null(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_92);
		if (!L_93)
		{
			G_B34_0 = L_91;
			G_B34_1 = L_90;
			goto IL_0335;
		}
		G_B33_0 = L_91;
		G_B33_1 = L_90;
	}
	{
		il2cpp_codegen_memcpy(L_94, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		RuntimeObject* L_95 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_94);
		NullCheck(((AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D*)CastclassClass((RuntimeObject*)L_95, AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_il2cpp_TypeInfo_var)));
		intptr_t L_96;
		L_96 = AndroidJavaProxy_GetRawProxy_m685E066A4D378B596CD88385B954AE90CBF328A9(((AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D*)CastclassClass((RuntimeObject*)L_95, AndroidJavaProxy_tE5521F9761F7B95444B9C39FB15FDFC23F80A78D_il2cpp_TypeInfo_var)), NULL);
		G_B35_0 = L_96;
		G_B35_1 = G_B33_0;
		G_B35_2 = G_B33_1;
		goto IL_033a;
	}

IL_0335:
	{
		G_B35_0 = 0;
		G_B35_1 = G_B34_0;
		G_B35_2 = G_B34_1;
	}

IL_033a:
	{
		AndroidJNISafe_SetStaticObjectField_m7757F7E30F8122DAF89F138A8AE727CB896BC721(G_B35_2, G_B35_1, G_B35_0, NULL);
		return;
	}

IL_0340:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_97 = { reinterpret_cast<intptr_t> (&il2cpp_defaults.array_class->byval_arg) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_98;
		L_98 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_97, NULL);
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_99 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		Type_t* L_100;
		L_100 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_99, NULL);
		il2cpp_codegen_runtime_class_init_inline(AndroidReflection_tD59014B286F902906DBB75DA3473897D35684908_il2cpp_TypeInfo_var);
		bool L_101;
		L_101 = AndroidReflection_IsAssignableFrom_mBAE0D5121AD208959B89E9BDAF62F0E0BAB917C6(L_98, L_100, NULL);
		if (!L_101)
		{
			goto IL_037f;
		}
	}
	{
		il2cpp_codegen_memcpy(L_102, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_tCE4037CFD91325CEB0ECC8FFA61A181595DF67F7);
		RuntimeObject* L_103 = Box(il2cpp_rgctx_data_no_init(method->rgctx_data, 1), L_102);
		intptr_t L_104;
		L_104 = AndroidJNIHelper_ConvertToJNIArray_mBEAE4605FF297D19AFB8CE4E8443C9C0F87E9A13(((RuntimeArray*)CastclassClass((RuntimeObject*)L_103, il2cpp_defaults.array_class)), NULL);
		V_0 = L_104;
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_105 = __this->___m_jclass;
		intptr_t L_106;
		L_106 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_105, NULL);
		intptr_t L_107 = ___0_fieldID;
		intptr_t L_108 = V_0;
		AndroidJNISafe_SetStaticObjectField_m7757F7E30F8122DAF89F138A8AE727CB896BC721(L_106, L_107, L_108, NULL);
		return;
	}

IL_037f:
	{
		RuntimeTypeHandle_t332A452B8B6179E4469B69525D0FE82A88030F7B L_109 = { reinterpret_cast<intptr_t> (il2cpp_rgctx_type(method->rgctx_data, 0)) };
		il2cpp_codegen_runtime_class_init_inline(il2cpp_defaults.systemtype_class);
		Type_t* L_110;
		L_110 = Type_GetTypeFromHandle_m6062B81682F79A4D6DF2640692EE6D9987858C57(L_109, NULL);
		Type_t* L_111 = L_110;
		if (L_111)
		{
			G_B40_0 = L_111;
			G_B40_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
			goto IL_0395;
		}
		G_B39_0 = L_111;
		G_B39_1 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral45BE536C9F5116AD2AF9F35AA7EF84EC0980CA92));
	}
	{
		G_B41_0 = ((String_t*)(NULL));
		G_B41_1 = G_B39_1;
		goto IL_039a;
	}

IL_0395:
	{
		NullCheck((RuntimeObject*)G_B40_0);
		String_t* L_112;
		L_112 = VirtualFuncInvoker0< String_t* >::Invoke(3, (RuntimeObject*)G_B40_0);
		G_B41_0 = L_112;
		G_B41_1 = G_B40_1;
	}

IL_039a:
	{
		String_t* L_113;
		L_113 = String_Concat_m8855A6DE10F84DA7F4EC113CADDB59873A25573B(G_B41_1, G_B41_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral42646B33B50B6AA15E22733C8900716F0FE19E1D)), NULL);
		Exception_t* L_114 = (Exception_t*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&Exception_t_il2cpp_TypeInfo_var)));
		Exception__ctor_m9B2BD92CD68916245A75109105D9071C9D430E7F(L_114, L_113, NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_114, method);
	}

IL_03aa:
	{
		return;
	}
}
// Method Definition Index: 63082
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AndroidJavaObject__SetStatic_TisIl2CppFullySharedGenericAny_mA923EF05918D13946E2452B81B496229D8B952FB_gshared (AndroidJavaObject_t8FFB930F335C1178405B82AC2BF512BB1EEF9EB0* __this, String_t* ___0_fieldName, Il2CppFullySharedGenericAny ___1_val, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_FieldType_t0374C27CAE86F7DA1E32463BA4A51CE6B68D02E1 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_FieldType_t0374C27CAE86F7DA1E32463BA4A51CE6B68D02E1);
	//<source_info:<no-source>:1>
	intptr_t V_0;
	memset((&V_0), 0, sizeof(V_0));
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = __this->___m_jclass;
		intptr_t L_1;
		L_1 = GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline(L_0, NULL);
		String_t* L_2 = ___0_fieldName;
		intptr_t L_3;
		L_3 = ((  intptr_t (*) (intptr_t, String_t*, bool, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 0)))(L_1, L_2, (bool)1, il2cpp_rgctx_method(method->rgctx_data, 0));
		V_0 = L_3;
		intptr_t L_4 = V_0;
		il2cpp_codegen_memcpy(L_5, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_val : &___1_val), SizeOf_FieldType_t0374C27CAE86F7DA1E32463BA4A51CE6B68D02E1);
		InvokerActionInvoker2< intptr_t, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), __this, L_4, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_5: *(void**)L_5));
		return;
	}
}
// Method Definition Index: 67869
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR Il2CppSharedGenericObject* AppConfigExtensions_GetState_TisIl2CppSharedGenericObject_mD2073E45A8E79704049E302AF508A336B20FA3AB_gshared (RuntimeObject* ___0_app, int32_t ___1_state, Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* ___2_store, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	bool V_0 = false;
	RuntimeObject* V_1 = NULL;
	bool V_2 = false;
	String_t* V_3 = NULL;
	Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* V_4 = NULL;
	Il2CppSharedGenericObject* V_5 = NULL;
	bool V_6 = false;
	bool V_7 = false;
	bool V_8 = false;
	Il2CppSharedGenericObject* V_9 = NULL;
	Il2CppSharedGenericObject* V_10 = NULL;
	{
		RuntimeObject* L_0 = ___0_app;
		V_0 = (bool)((((RuntimeObject*)(RuntimeObject*)L_0) == ((RuntimeObject*)(RuntimeObject*)NULL))? 1 : 0);
		bool L_1 = V_0;
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		RuntimeObject* L_2;
		L_2 = FirebaseHandler_get_AppUtils_m5D80C76317AFA8DBEEFEF2427573A6EE7B6F7B27_inline(NULL);
		NullCheck(L_2);
		RuntimeObject* L_3;
		L_3 = InterfaceFuncInvoker0< RuntimeObject* >::Invoke(2, IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var, L_2);
		___0_app = L_3;
	}

IL_0017:
	{
		il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		RuntimeObject* L_4 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Sync;
		V_1 = L_4;
		V_2 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0091:
			{
				{
					bool L_5 = V_2;
					if (!L_5)
					{
						goto IL_009b;
					}
				}
				{
					RuntimeObject* L_6 = V_1;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA(L_6, NULL);
				}

IL_009b:
				{
					return;
				}
			}
		});
		try
		{
			{
				RuntimeObject* L_7 = V_1;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149(L_7, (&V_2), NULL);
				RuntimeObject* L_8 = ___0_app;
				NullCheck(L_8);
				String_t* L_9;
				L_9 = InterfaceFuncInvoker0< String_t* >::Invoke(1, IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var, L_8);
				V_3 = L_9;
				String_t* L_10 = V_3;
				bool L_11;
				L_11 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_10, NULL);
				V_6 = L_11;
				bool L_12 = V_6;
				if (!L_12)
				{
					goto IL_0044_1;
				}
			}
			{
				il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
				String_t* L_13 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Default;
				V_3 = L_13;
			}

IL_0044_1:
			{
				Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* L_14 = ___2_store;
				int32_t L_15 = ___1_state;
				NullCheck(L_14);
				bool L_16;
				L_16 = Dictionary_2_TryGetValue_m9DCAC5074133326FBC20B4802AFD2E5761C18D80(L_14, L_15, (&V_4), il2cpp_rgctx_method(method->rgctx_data, 1));
				V_7 = (bool)((((int32_t)L_16) == ((int32_t)0))? 1 : 0);
				bool L_17 = V_7;
				if (!L_17)
				{
					goto IL_0069_1;
				}
			}
			{
				Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* L_18 = (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 3));
				Dictionary_2__ctor_m62A668AAEBEB00987A654F7755EFCCEEF5756DD3(L_18, il2cpp_rgctx_method(method->rgctx_data, 4));
				V_4 = L_18;
				Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* L_19 = ___2_store;
				int32_t L_20 = ___1_state;
				Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* L_21 = V_4;
				NullCheck(L_19);
				Dictionary_2_set_Item_m97C74ADCBF64325E94417D123CA992CAE1EE3D01(L_19, L_20, L_21, il2cpp_rgctx_method(method->rgctx_data, 5));
			}

IL_0069_1:
			{
				Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* L_22 = V_4;
				String_t* L_23 = V_3;
				NullCheck(L_22);
				bool L_24;
				L_24 = Dictionary_2_TryGetValue_m066DE45CEB9B026DD250BB30F6365406F9FD9EFD(L_22, L_23, (&V_5), il2cpp_rgctx_method(method->rgctx_data, 6));
				V_8 = (bool)((((int32_t)L_24) == ((int32_t)0))? 1 : 0);
				bool L_25 = V_8;
				if (!L_25)
				{
					goto IL_008b_1;
				}
			}
			{
				il2cpp_codegen_initobj((&V_9), sizeof(Il2CppSharedGenericObject*));
				Il2CppSharedGenericObject* L_26 = V_9;
				V_10 = L_26;
				goto IL_009c;
			}

IL_008b_1:
			{
				Il2CppSharedGenericObject* L_27 = V_5;
				V_10 = L_27;
				goto IL_009c;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_009c:
	{
		Il2CppSharedGenericObject* L_28 = V_10;
		return L_28;
	}
}
// Method Definition Index: 67869
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AppConfigExtensions_GetState_TisIl2CppFullySharedGenericAny_m64084357B28DFBEAD5F45F6FEEEC2695D92DCE5A_gshared (RuntimeObject* ___0_app, int32_t ___1_state, Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29* ___2_store, Il2CppFullySharedGenericAny* il2cppRetVal, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 8));
	const Il2CppFullySharedGenericAny L_26 = alloca(SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
	const Il2CppFullySharedGenericAny L_27 = L_26;
	const Il2CppFullySharedGenericAny L_28 = L_26;
	//<source_info:<no-source>:1>
	bool V_0 = false;
	RuntimeObject* V_1 = NULL;
	bool V_2 = false;
	String_t* V_3 = NULL;
	Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* V_4 = NULL;
	Il2CppFullySharedGenericAny V_5 = alloca(SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
	memset(V_5, 0, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
	bool V_6 = false;
	bool V_7 = false;
	bool V_8 = false;
	Il2CppFullySharedGenericAny V_9 = alloca(SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
	memset(V_9, 0, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
	Il2CppFullySharedGenericAny V_10 = alloca(SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
	memset(V_10, 0, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
	{
		RuntimeObject* L_0 = ___0_app;
		V_0 = (bool)((((RuntimeObject*)(RuntimeObject*)L_0) == ((RuntimeObject*)(RuntimeObject*)NULL))? 1 : 0);
		bool L_1 = V_0;
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		RuntimeObject* L_2;
		L_2 = FirebaseHandler_get_AppUtils_m5D80C76317AFA8DBEEFEF2427573A6EE7B6F7B27_inline(NULL);
		NullCheck(L_2);
		RuntimeObject* L_3;
		L_3 = InterfaceFuncInvoker0< RuntimeObject* >::Invoke(2, IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var, L_2);
		___0_app = L_3;
	}

IL_0017:
	{
		il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		RuntimeObject* L_4 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Sync;
		V_1 = L_4;
		V_2 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0091:
			{
				{
					bool L_5 = V_2;
					if (!L_5)
					{
						goto IL_009b;
					}
				}
				{
					RuntimeObject* L_6 = V_1;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA(L_6, NULL);
				}

IL_009b:
				{
					return;
				}
			}
		});
		try
		{
			{
				RuntimeObject* L_7 = V_1;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149(L_7, (&V_2), NULL);
				RuntimeObject* L_8 = ___0_app;
				NullCheck(L_8);
				String_t* L_9;
				L_9 = InterfaceFuncInvoker0< String_t* >::Invoke(1, IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var, L_8);
				V_3 = L_9;
				String_t* L_10 = V_3;
				bool L_11;
				L_11 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_10, NULL);
				V_6 = L_11;
				bool L_12 = V_6;
				if (!L_12)
				{
					goto IL_0044_1;
				}
			}
			{
				il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
				String_t* L_13 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Default;
				V_3 = L_13;
			}

IL_0044_1:
			{
				Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29* L_14 = ___2_store;
				int32_t L_15 = ___1_state;
				NullCheck(L_14);
				bool L_16;
				L_16 = InvokerFuncInvoker2< bool, int32_t, Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053** >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), L_14, L_15, (&V_4));
				V_7 = (bool)((((int32_t)L_16) == ((int32_t)0))? 1 : 0);
				bool L_17 = V_7;
				if (!L_17)
				{
					goto IL_0069_1;
				}
			}
			{
				Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* L_18 = (Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 3));
				((  void (*) (Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 4)))(L_18, il2cpp_rgctx_method(method->rgctx_data, 4));
				V_4 = L_18;
				Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29* L_19 = ___2_store;
				int32_t L_20 = ___1_state;
				Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* L_21 = V_4;
				NullCheck(L_19);
				InvokerActionInvoker2< int32_t, Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 5)), il2cpp_rgctx_method(method->rgctx_data, 5), L_19, L_20, L_21);
			}

IL_0069_1:
			{
				Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* L_22 = V_4;
				String_t* L_23 = V_3;
				NullCheck(L_22);
				bool L_24;
				L_24 = InvokerFuncInvoker2< bool, String_t*, Il2CppFullySharedGenericAny* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 6)), il2cpp_rgctx_method(method->rgctx_data, 6), L_22, L_23, (Il2CppFullySharedGenericAny*)V_5);
				V_8 = (bool)((((int32_t)L_24) == ((int32_t)0))? 1 : 0);
				bool L_25 = V_8;
				if (!L_25)
				{
					goto IL_008b_1;
				}
			}
			{
				il2cpp_codegen_initobj((Il2CppFullySharedGenericAny*)V_9, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
				il2cpp_codegen_memcpy(L_26, V_9, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
				il2cpp_codegen_memcpy(V_10, L_26, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
				goto IL_009c;
			}

IL_008b_1:
			{
				il2cpp_codegen_memcpy(L_27, V_5, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
				il2cpp_codegen_memcpy(V_10, L_27, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
				goto IL_009c;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_009c:
	{
		il2cpp_codegen_memcpy(L_28, V_10, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
		il2cpp_codegen_memcpy(il2cppRetVal, L_28, SizeOf_T_tF8CF14F6855A878F015F1E8C076C3D7EF06A2CE2);
		return;
	}
}
// Method Definition Index: 67870
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AppConfigExtensions_SetState_TisIl2CppSharedGenericObject_mE9AE29863A7F31E598912BFAA5F566B4A589AEED_gshared (RuntimeObject* ___0_app, int32_t ___1_state, Il2CppSharedGenericObject* ___2_value, Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* ___3_store, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	//<source_info:<no-source>:1>
	bool V_0 = false;
	RuntimeObject* V_1 = NULL;
	bool V_2 = false;
	String_t* V_3 = NULL;
	Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* V_4 = NULL;
	bool V_5 = false;
	bool V_6 = false;
	{
		RuntimeObject* L_0 = ___0_app;
		V_0 = (bool)((((RuntimeObject*)(RuntimeObject*)L_0) == ((RuntimeObject*)(RuntimeObject*)NULL))? 1 : 0);
		bool L_1 = V_0;
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		RuntimeObject* L_2;
		L_2 = FirebaseHandler_get_AppUtils_m5D80C76317AFA8DBEEFEF2427573A6EE7B6F7B27_inline(NULL);
		NullCheck(L_2);
		RuntimeObject* L_3;
		L_3 = InterfaceFuncInvoker0< RuntimeObject* >::Invoke(2, IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var, L_2);
		___0_app = L_3;
	}

IL_0017:
	{
		il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		RuntimeObject* L_4 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Sync;
		V_1 = L_4;
		V_2 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0076:
			{
				{
					bool L_5 = V_2;
					if (!L_5)
					{
						goto IL_0080;
					}
				}
				{
					RuntimeObject* L_6 = V_1;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA(L_6, NULL);
				}

IL_0080:
				{
					return;
				}
			}
		});
		try
		{
			{
				RuntimeObject* L_7 = V_1;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149(L_7, (&V_2), NULL);
				RuntimeObject* L_8 = ___0_app;
				NullCheck(L_8);
				String_t* L_9;
				L_9 = InterfaceFuncInvoker0< String_t* >::Invoke(1, IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var, L_8);
				V_3 = L_9;
				String_t* L_10 = V_3;
				bool L_11;
				L_11 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_10, NULL);
				V_5 = L_11;
				bool L_12 = V_5;
				if (!L_12)
				{
					goto IL_0044_1;
				}
			}
			{
				il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
				String_t* L_13 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Default;
				V_3 = L_13;
			}

IL_0044_1:
			{
				Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* L_14 = ___3_store;
				int32_t L_15 = ___1_state;
				NullCheck(L_14);
				bool L_16;
				L_16 = Dictionary_2_TryGetValue_m9DCAC5074133326FBC20B4802AFD2E5761C18D80(L_14, L_15, (&V_4), il2cpp_rgctx_method(method->rgctx_data, 1));
				V_6 = (bool)((((int32_t)L_16) == ((int32_t)0))? 1 : 0);
				bool L_17 = V_6;
				if (!L_17)
				{
					goto IL_0069_1;
				}
			}
			{
				Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* L_18 = (Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 3));
				Dictionary_2__ctor_m62A668AAEBEB00987A654F7755EFCCEEF5756DD3(L_18, il2cpp_rgctx_method(method->rgctx_data, 4));
				V_4 = L_18;
				Dictionary_2_t91CF7E3FF88616124B31822ED05926AA9593938B* L_19 = ___3_store;
				int32_t L_20 = ___1_state;
				Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* L_21 = V_4;
				NullCheck(L_19);
				Dictionary_2_set_Item_m97C74ADCBF64325E94417D123CA992CAE1EE3D01(L_19, L_20, L_21, il2cpp_rgctx_method(method->rgctx_data, 5));
			}

IL_0069_1:
			{
				Dictionary_2_tF2E55EA2A81F3D5E555A97234AFC9A2CADA5D415* L_22 = V_4;
				String_t* L_23 = V_3;
				Il2CppSharedGenericObject* L_24 = ___2_value;
				NullCheck(L_22);
				Dictionary_2_set_Item_m5B309AB302243C55150F6B418CC3C35D16E908C0(L_22, L_23, L_24, il2cpp_rgctx_method(method->rgctx_data, 7));
				goto IL_0081;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0081:
	{
		return;
	}
}
// Method Definition Index: 67870
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR void AppConfigExtensions_SetState_TisIl2CppFullySharedGenericAny_mB7D11C81ECED7DA072C6FE0CC6081EE1683B8B3A_gshared (RuntimeObject* ___0_app, int32_t ___1_state, Il2CppFullySharedGenericAny ___2_value, Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29* ___3_store, const RuntimeMethod* method) 
{
	if (!il2cpp_rgctx_is_initialized(method))
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var);
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var);
		il2cpp_rgctx_method_init(method);
	}
	const uint32_t SizeOf_T_t066344396F6066855233E20C60D9D55AF8BDFD1F = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 6));
	const Il2CppFullySharedGenericAny L_24 = alloca(SizeOf_T_t066344396F6066855233E20C60D9D55AF8BDFD1F);
	//<source_info:<no-source>:1>
	bool V_0 = false;
	RuntimeObject* V_1 = NULL;
	bool V_2 = false;
	String_t* V_3 = NULL;
	Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* V_4 = NULL;
	bool V_5 = false;
	bool V_6 = false;
	{
		RuntimeObject* L_0 = ___0_app;
		V_0 = (bool)((((RuntimeObject*)(RuntimeObject*)L_0) == ((RuntimeObject*)(RuntimeObject*)NULL))? 1 : 0);
		bool L_1 = V_0;
		if (!L_1)
		{
			goto IL_0017;
		}
	}
	{
		il2cpp_codegen_runtime_class_init_inline(FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		RuntimeObject* L_2;
		L_2 = FirebaseHandler_get_AppUtils_m5D80C76317AFA8DBEEFEF2427573A6EE7B6F7B27_inline(NULL);
		NullCheck(L_2);
		RuntimeObject* L_3;
		L_3 = InterfaceFuncInvoker0< RuntimeObject* >::Invoke(2, IFirebaseAppUtils_t61EDF19372DFE7348E02194135E2F3B8801E3391_il2cpp_TypeInfo_var, L_2);
		___0_app = L_3;
	}

IL_0017:
	{
		il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
		RuntimeObject* L_4 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Sync;
		V_1 = L_4;
		V_2 = (bool)0;
	}
	{
		auto __finallyBlock = il2cpp::utils::Finally([&]
		{

FINALLY_0076:
			{
				{
					bool L_5 = V_2;
					if (!L_5)
					{
						goto IL_0080;
					}
				}
				{
					RuntimeObject* L_6 = V_1;
					Monitor_Exit_m05B2CF037E2214B3208198C282490A2A475653FA(L_6, NULL);
				}

IL_0080:
				{
					return;
				}
			}
		});
		try
		{
			{
				RuntimeObject* L_7 = V_1;
				Monitor_Enter_m3CDB589DA1300B513D55FDCFB52B63E879794149(L_7, (&V_2), NULL);
				RuntimeObject* L_8 = ___0_app;
				NullCheck(L_8);
				String_t* L_9;
				L_9 = InterfaceFuncInvoker0< String_t* >::Invoke(1, IFirebaseAppPlatform_tB44DDF88ADD112DC55E9C5EF84C774D244C8C228_il2cpp_TypeInfo_var, L_8);
				V_3 = L_9;
				String_t* L_10 = V_3;
				bool L_11;
				L_11 = String_IsNullOrEmpty_mEA9E3FB005AC28FE02E69FCF95A7B8456192B478(L_10, NULL);
				V_5 = L_11;
				bool L_12 = V_5;
				if (!L_12)
				{
					goto IL_0044_1;
				}
			}
			{
				il2cpp_codegen_runtime_class_init_inline(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var);
				String_t* L_13 = ((AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_StaticFields*)il2cpp_codegen_static_fields_for(AppConfigExtensions_t6B8627CD4EFF8F05D2F749CC406E5E12F04CEE48_il2cpp_TypeInfo_var))->___Default;
				V_3 = L_13;
			}

IL_0044_1:
			{
				Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29* L_14 = ___3_store;
				int32_t L_15 = ___1_state;
				NullCheck(L_14);
				bool L_16;
				L_16 = InvokerFuncInvoker2< bool, int32_t, Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053** >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 1)), il2cpp_rgctx_method(method->rgctx_data, 1), L_14, L_15, (&V_4));
				V_6 = (bool)((((int32_t)L_16) == ((int32_t)0))? 1 : 0);
				bool L_17 = V_6;
				if (!L_17)
				{
					goto IL_0069_1;
				}
			}
			{
				Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* L_18 = (Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 3));
				((  void (*) (Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 4)))(L_18, il2cpp_rgctx_method(method->rgctx_data, 4));
				V_4 = L_18;
				Dictionary_2_tAEFECB7912C437DA011A30BE6FCDFBF86D4D0B29* L_19 = ___3_store;
				int32_t L_20 = ___1_state;
				Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* L_21 = V_4;
				NullCheck(L_19);
				InvokerActionInvoker2< int32_t, Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 5)), il2cpp_rgctx_method(method->rgctx_data, 5), L_19, L_20, L_21);
			}

IL_0069_1:
			{
				Dictionary_2_t0E53A4C96A42ED436853608F2A41D26180F25053* L_22 = V_4;
				String_t* L_23 = V_3;
				il2cpp_codegen_memcpy(L_24, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 6)) ? ___2_value : &___2_value), SizeOf_T_t066344396F6066855233E20C60D9D55AF8BDFD1F);
				NullCheck(L_22);
				InvokerActionInvoker2< String_t*, Il2CppFullySharedGenericAny >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 7)), il2cpp_rgctx_method(method->rgctx_data, 7), L_22, L_23, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 6)) ? L_24: *(void**)L_24));
				goto IL_0081;
			}
		}
		catch(Il2CppNativeThreadAbortException&)
		{
			__finallyBlock.SetNativeThreadAbortOccurred();
		}
		catch(Il2CppExceptionWrapper& e)
		{
			__finallyBlock.StoreException(e.ex);
		}
	}

IL_0081:
	{
		return;
	}
}
// Method Definition Index: 2649
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050* Array_AsReadOnly_TisCustomAttributeNamedArgument_t4EC1C2BB9943BEB7E77AC0870BE2A899E23B4E02_mD88B0D043B3113058F0A90CB78BB75E6716BE17A_gshared (CustomAttributeNamedArgumentU5BU5D_tC0A39D9401E28662213F5958EFF5D26D0681B440* ___0_array, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		CustomAttributeNamedArgumentU5BU5D_tC0A39D9401E28662213F5958EFF5D26D0681B440* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		CustomAttributeNamedArgumentU5BU5D_tC0A39D9401E28662213F5958EFF5D26D0681B440* L_2 = ___0_array;
		ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050* L_3 = (ReadOnlyCollection_1_t183E854D701353CDB0176A7146736A0BC505B050*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		ReadOnlyCollection_1__ctor_m010AA1E4CD3820BCC07B77A482D6A17D5C7F14C2(L_3, (RuntimeObject*)L_2, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_3;
	}
}
// Method Definition Index: 2649
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513* Array_AsReadOnly_TisCustomAttributeTypedArgument_tAAA19ADE66B16A67D030C8C67D7ADB29A7BEC75F_mC945FB68604FAA2CFA7F469D9D420F3F2830F037_gshared (CustomAttributeTypedArgumentU5BU5D_t6CAA09EC6AACBED57FC8B02C587D50BF6B738C6B* ___0_array, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		CustomAttributeTypedArgumentU5BU5D_t6CAA09EC6AACBED57FC8B02C587D50BF6B738C6B* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		CustomAttributeTypedArgumentU5BU5D_t6CAA09EC6AACBED57FC8B02C587D50BF6B738C6B* L_2 = ___0_array;
		ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513* L_3 = (ReadOnlyCollection_1_tD22937E99FBFBC6FD85A9802376E389578922513*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		ReadOnlyCollection_1__ctor_mE22404DEC336AFE9817BF78FF9F7F90E07EE4F7A(L_3, (RuntimeObject*)L_2, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_3;
	}
}
// Method Definition Index: 2649
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263* Array_AsReadOnly_TisIl2CppSharedGenericObject_m3941F8A9233EDADA7E60EB1618BBBE58C11C329D_gshared (__CanonU5BU5D_tFF96AE6C231BB36A6CEE54CEEB72ED8E90201979* ___0_array, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		__CanonU5BU5D_tFF96AE6C231BB36A6CEE54CEEB72ED8E90201979* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		__CanonU5BU5D_tFF96AE6C231BB36A6CEE54CEEB72ED8E90201979* L_2 = ___0_array;
		ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263* L_3 = (ReadOnlyCollection_1_t06F71F2F3EBC6E0A34714E0A7EB3367B6D248263*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		ReadOnlyCollection_1__ctor_mC1890FAC00703F47A655C35CBCB613C74A811580(L_3, (RuntimeObject*)L_2, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_3;
	}
}
// Method Definition Index: 2649
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR ReadOnlyCollection_1_t5B7AA4E006906DE6818A44873F2D5987EFBF3AB8* Array_AsReadOnly_TisIl2CppFullySharedGenericAny_mC6D4C312AF13E636588C50683CC2F2054B6298A4_gshared (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_2 = ___0_array;
		ReadOnlyCollection_1_t5B7AA4E006906DE6818A44873F2D5987EFBF3AB8* L_3 = (ReadOnlyCollection_1_t5B7AA4E006906DE6818A44873F2D5987EFBF3AB8*)il2cpp_codegen_object_new(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
		((  void (*) (ReadOnlyCollection_1_t5B7AA4E006906DE6818A44873F2D5987EFBF3AB8*, RuntimeObject*, const RuntimeMethod*))il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)))(L_3, (RuntimeObject*)L_2, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_3;
	}
}
// Method Definition Index: 2688
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_mCA6AA8E08561CD273B57726185CFB10006073270_gshared (UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* ___0_array, uint64_t ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_2 = ___0_array;
		UInt64U5BU5D_tAB1A62450AC0899188486EDB9FC066B8BEED9299* L_3 = ___0_array;
		NullCheck(L_3);
		int32_t L_4 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_3)->max_length),NULL));
		uint64_t L_5 = ___1_value;
		int32_t L_6;
		L_6 = Array_BinarySearch_TisUInt64_t8F12534CC8FC4B5860F2A2CD1EE79D322E7A41AF_m0AA0BC859F6008F37B680B00508C065B098C7D9A(L_2, 0, L_4, L_5, (RuntimeObject*)NULL, il2cpp_rgctx_method(method->rgctx_data, 2));
		return L_6;
	}
}
// Method Definition Index: 2688
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisIl2CppFullySharedGenericAny_mD7255DC86A3D47AD46661F249A70FB7D7FA0673C_gshared (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, Il2CppFullySharedGenericAny ___1_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t14FAB5FC7A26C6611985105C3DFB474A16FF6AC1 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_T_t14FAB5FC7A26C6611985105C3DFB474A16FF6AC1);
	//<source_info:<no-source>:1>
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_2 = ___0_array;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___0_array;
		NullCheck(L_3);
		int32_t L_4 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_3)->max_length),NULL));
		il2cpp_codegen_memcpy(L_5, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_value : &___1_value), SizeOf_T_t14FAB5FC7A26C6611985105C3DFB474A16FF6AC1);
		int32_t L_6;
		L_6 = InvokerFuncInvoker5< int32_t, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*, int32_t, int32_t, Il2CppFullySharedGenericAny, RuntimeObject* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_2, 0, L_4, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_5: *(void**)L_5), (RuntimeObject*)NULL);
		return L_6;
	}
}
// Method Definition Index: 2689
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisIl2CppFullySharedGenericAny_m7E39322AE8312F6EB3768EE8329A0236270DB35B_gshared (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, Il2CppFullySharedGenericAny ___1_value, RuntimeObject* ___2_comparer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t75EC08BCC087A0780793950F3A77D634501D99B2 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_5 = alloca(SizeOf_T_t75EC08BCC087A0780793950F3A77D634501D99B2);
	//<source_info:<no-source>:1>
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_2 = ___0_array;
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_3 = ___0_array;
		NullCheck(L_3);
		int32_t L_4 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_3)->max_length),NULL));
		il2cpp_codegen_memcpy(L_5, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___1_value : &___1_value), SizeOf_T_t75EC08BCC087A0780793950F3A77D634501D99B2);
		RuntimeObject* L_6 = ___2_comparer;
		int32_t L_7;
		L_7 = InvokerFuncInvoker5< int32_t, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*, int32_t, int32_t, Il2CppFullySharedGenericAny, RuntimeObject* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 3)), il2cpp_rgctx_method(method->rgctx_data, 3), NULL, L_2, 0, L_4, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_5: *(void**)L_5), L_6);
		return L_7;
	}
}
// Method Definition Index: 2689
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisSortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53_m4EFAF15B23ECDD6CF265D9EDF30FEF23096FA1D1_gshared (SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B* ___0_array, SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 ___1_value, RuntimeObject* ___2_comparer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	{
		SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B* L_2 = ___0_array;
		SortNodeU5BU5D_t93BC9DAFE9D4796A15C2DAEEAA4799B6D0A6179B* L_3 = ___0_array;
		NullCheck(L_3);
		int32_t L_4 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_3)->max_length),NULL));
		SortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53 L_5 = ___1_value;
		RuntimeObject* L_6 = ___2_comparer;
		int32_t L_7;
		L_7 = Array_BinarySearch_TisSortNode_t6A06A977B925BF3F2ECC51B7C74B7A9AE8CE5C53_m8F7822162DB61B62B083C2CD23E5F632B85BF601(L_2, 0, L_4, L_5, L_6, il2cpp_rgctx_method(method->rgctx_data, 3));
		return L_7;
	}
}
// Method Definition Index: 2690
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisIl2CppFullySharedGenericAny_m41E7B9D01239068411C6C8644D275AD08C1BEBF1_gshared (__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* ___0_array, int32_t ___1_index, int32_t ___2_length, Il2CppFullySharedGenericAny ___3_value, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	const uint32_t SizeOf_T_t718F56072021E2C1F6A1F91F772E7ABDFACEEAC0 = il2cpp_codegen_sizeof(il2cpp_rgctx_data_no_init(method->rgctx_data, 1));
	const Il2CppFullySharedGenericAny L_3 = alloca(SizeOf_T_t718F56072021E2C1F6A1F91F772E7ABDFACEEAC0);
	//<source_info:<no-source>:1>
	{
		__Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC* L_0 = ___0_array;
		int32_t L_1 = ___1_index;
		int32_t L_2 = ___2_length;
		il2cpp_codegen_memcpy(L_3, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? ___3_value : &___3_value), SizeOf_T_t718F56072021E2C1F6A1F91F772E7ABDFACEEAC0);
		int32_t L_4;
		L_4 = InvokerFuncInvoker5< int32_t, __Il2CppFullySharedGenericTypeU5BU5D_tCAB6D060972DD49223A834B7EEFEB9FE2D003BEC*, int32_t, int32_t, Il2CppFullySharedGenericAny, RuntimeObject* >::Invoke(il2cpp_codegen_get_direct_method_pointer(il2cpp_rgctx_method(method->rgctx_data, 2)), il2cpp_rgctx_method(method->rgctx_data, 2), NULL, L_0, L_1, L_2, (il2cpp_codegen_class_is_value_type(il2cpp_rgctx_data_no_init(method->rgctx_data, 1)) ? L_3: *(void**)L_3), (RuntimeObject*)NULL);
		return L_4;
	}
}
// Method Definition Index: 2691
IL2CPP_EXTERN_C IL2CPP_METHOD_ATTR int32_t Array_BinarySearch_TisCopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565_m8B18132CAF691B1212F171E486CE38EB7AD377F1_gshared (CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466* ___0_array, int32_t ___1_index, int32_t ___2_length, CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 ___3_value, RuntimeObject* ___4_comparer, const RuntimeMethod* method) 
{
	il2cpp_rgctx_method_init(method);
	//<source_info:<no-source>:1>
	String_t* G_B7_0 = NULL;
	{
		CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466* L_0 = ___0_array;
		if (L_0)
		{
			goto IL_000e;
		}
	}
	{
		ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129* L_1 = (ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentNullException_t327031E412FAB2351B0022DD5DAD47E67E597129_il2cpp_TypeInfo_var)));
		ArgumentNullException__ctor_m444AE141157E333844FC1A9500224C2F9FD24F4B(L_1, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralB829404B947F7E1629A30B5E953A49EB21CCD2ED)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_1, method);
	}

IL_000e:
	{
		int32_t L_2 = ___1_index;
		if ((((int32_t)L_2) < ((int32_t)0)))
		{
			goto IL_0016;
		}
	}
	{
		int32_t L_3 = ___2_length;
		if ((((int32_t)L_3) >= ((int32_t)0)))
		{
			goto IL_0031;
		}
	}

IL_0016:
	{
		int32_t L_4 = ___1_index;
		if ((((int32_t)L_4) < ((int32_t)0)))
		{
			goto IL_0021;
		}
	}
	{
		G_B7_0 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteralE8744A8B8BD390EB66CA0CAE2376C973E6904FFB));
		goto IL_0026;
	}

IL_0021:
	{
		G_B7_0 = ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral2B6D6F48C27C60C3B55391AB377D9DC8F5639AA1));
	}

IL_0026:
	{
		ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F* L_5 = (ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentOutOfRangeException_tEA2822DAF62B10EEED00E0E3A341D4BAF78CF85F_il2cpp_TypeInfo_var)));
		ArgumentOutOfRangeException__ctor_mE5B2755F0BEA043CACF915D5CE140859EE58FA66(L_5, G_B7_0, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral38E3DBC7FC353425EF3A98DC8DAC6689AF5FD1BE)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_5, method);
	}

IL_0031:
	{
		CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466* L_6 = ___0_array;
		NullCheck(L_6);
		int32_t L_7 = (il2cpp_codegen_conv<int32_t,int64_t,int64_t,false,false>((((RuntimeArray*)L_6)->max_length),NULL));
		int32_t L_8 = ___1_index;
		int32_t L_9 = ___2_length;
		if ((((int32_t)((int32_t)il2cpp_codegen_subtract(L_7, L_8))) >= ((int32_t)L_9)))
		{
			goto IL_0044;
		}
	}
	{
		ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263* L_10 = (ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263*)il2cpp_codegen_object_new(((RuntimeClass*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&ArgumentException_tAD90411542A20A9C72D5CDA3A84181D8B947A263_il2cpp_TypeInfo_var)));
		ArgumentException__ctor_m026938A67AF9D36BB7ED27F80425D7194B514465(L_10, ((String_t*)il2cpp_codegen_initialize_runtime_metadata_inline((uintptr_t*)&_stringLiteral7F4C724BD10943E8B0B17A6E069F992E219EF5E8)), NULL);
		IL2CPP_RAISE_MANAGED_EXCEPTION(L_10, method);
	}

IL_0044:
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data_no_init(method->rgctx_data, 2));
		ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* L_11;
		L_11 = ArraySortHelper_1_get_Default_m9357CBDC001EFB2BA8D065940A5C9FEFED3CC036_inline(il2cpp_rgctx_method(method->rgctx_data, 1));
		CopyInfoU5BU5D_t99CD8AF5B14252370B2B7C05032486DF882AF466* L_12 = ___0_array;
		int32_t L_13 = ___1_index;
		int32_t L_14 = ___2_length;
		CopyInfo_tB2613726E308EEDC2AE910784FA68BBB0A771565 L_15 = ___3_value;
		RuntimeObject* L_16 = ___4_comparer;
		NullCheck(L_11);
		int32_t L_17;
		L_17 = ArraySortHelper_1_BinarySearch_m65877F6FF5C5E15405D3326440C99808F156B993(L_11, L_12, L_13, L_14, L_15, L_16, il2cpp_rgctx_method(method->rgctx_data, 6));
		return L_17;
	}
}
// Method Definition Index: 41157
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t AllocatorHandle_get_Value_m24A0A3E433794106E43E9140CC2BB55493C8F30F_inline (AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148* __this, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		uint16_t L_0 = __this->___Index;
		return (int32_t)L_0;
	}
}
// Method Definition Index: 43691
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 RewindableAllocator_get_Handle_mF81EDA2102485C46965AAB56347E8F64FE551D9E_inline (RewindableAllocator_tB18F8ADC8F2EE36E1F51FCCCFF0AC093108EF254* __this, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		AllocatorHandle_t3CA09720B1F89F91A8DDBA95E74C28A1EC3E3148 L_0 = __this->___m_handle;
		return L_0;
	}
}
// Method Definition Index: 3384
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool IntPtr_op_Equality_m7D9CDCDE9DC2A0C2C614633F4921E90187FAB271_inline (intptr_t ___0_value1, intptr_t ___1_value2, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_value1;
		intptr_t L_1 = ___1_value2;
		return (bool)((((intptr_t)L_0) == ((intptr_t)L_1))? 1 : 0);
	}
}
// Method Definition Index: 63008
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR intptr_t GlobalJavaObjectRef_op_Implicit_m444B263750F9B778C87C30EA918CDC0B62271F24_inline (GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* ___0_obj, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		GlobalJavaObjectRef_t20D8E5AAFC2EB2518FCABBF40465855E797FF0D8* L_0 = ___0_obj;
		NullCheck(L_0);
		intptr_t L_1 = L_0->___m_jobject;
		return L_1;
	}
}
// Method Definition Index: 67783
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR RuntimeObject* FirebaseHandler_get_AppUtils_m5D80C76317AFA8DBEEFEF2427573A6EE7B6F7B27_inline (const RuntimeMethod* method) 
{
	static bool s_Il2CppMethodInitialized;
	if (!s_Il2CppMethodInitialized)
	{
		il2cpp_codegen_initialize_runtime_metadata((uintptr_t*)&FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		s_Il2CppMethodInitialized = true;
	}
	//<source_info:<no-source>:1>
	{
		il2cpp_codegen_runtime_class_init_inline(FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var);
		RuntimeObject* L_0 = ((FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_StaticFields*)il2cpp_codegen_static_fields_for(FirebaseHandler_t11BC96204B8CDE75558E6BBDB2ED05FA2979B586_il2cpp_TypeInfo_var))->___U3CAppUtilsU3Ek__BackingField;
		return L_0;
	}
}
// Method Definition Index: 48689
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void FunctionPointer_1__ctor_mE3820F1453217EF8118D6A53E1B8CA7AFF1E463C_gshared_inline (FunctionPointer_1_t57A2ADDF748E323E76FF7DAD737F07BA9549251E* __this, intptr_t ___0_ptr, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		intptr_t L_0 = ___0_ptr;
		__this->____ptr = L_0;
		return;
	}
}
// Method Definition Index: 622
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool Func_1_Invoke_mBB7F37C468451AF57FAF31635C544D6B8C4373B2_gshared_inline (Func_1_t2BE7F58348C9CC544A8973B3A9E55541DE43C457* __this, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	typedef bool (*FunctionPointerType) (RuntimeObject*, const RuntimeMethod*);
	return ((FunctionPointerType)__this->___invoke_impl)((Il2CppObject*)__this->___method_code, reinterpret_cast<RuntimeMethod*>(__this->___method));
}
// Method Definition Index: 32488
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool NativeArray_1_get_IsCreated_mD74FCA194584E6EA7916853B62401EB78240A081_gshared_inline (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF* __this, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		void* L_0 = __this->___m_Buffer;
		uintptr_t L_1 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(0,NULL));
		return (bool)((((int32_t)((((intptr_t)L_0) == ((intptr_t)L_1))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}
}
// Method Definition Index: 32605
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void* NativeArrayUnsafeUtility_GetUnsafePtr_TisByte_t94D9231AC217BE4D2E004C4CD32DF6D099EA41A3_m8CFDB2DF56E810A2E2FB3686AF676FCAC65AFCC2_gshared_inline (NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF ___0_nativeArray, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		NativeArray_1_t81F55263465517B73C455D3400CF67B4BADD85CF L_0 = ___0_nativeArray;
		void* L_1 = L_0.___m_Buffer;
		return L_1;
	}
}
// Method Definition Index: 32488
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR bool NativeArray_1_get_IsCreated_mF1154ABCB79F43B3A71BFA8DCB85ACBDFA2838D5_gshared_inline (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190* __this, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		void* L_0 = __this->___m_Buffer;
		uintptr_t L_1 = (il2cpp_codegen_conv<uintptr_t,int32_t,int32_t,false,false>(0,NULL));
		return (bool)((((int32_t)((((intptr_t)L_0) == ((intptr_t)L_1))? 1 : 0)) == ((int32_t)0))? 1 : 0);
	}
}
// Method Definition Index: 32605
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void* NativeArrayUnsafeUtility_GetUnsafePtr_TisSByte_tFEFFEF5D2FEBF5207950AE6FAC150FC53B668DB5_mCD832837FF5CE235B506272835BA049D56968E2F_gshared_inline (NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 ___0_nativeArray, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		NativeArray_1_tC3AFDC9012293850DE671F2A1E55484968716190 L_0 = ___0_nativeArray;
		void* L_1 = L_0.___m_Buffer;
		return L_1;
	}
}
// Method Definition Index: 2062
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR void Span_1__ctor_mA426411A0F5450BCD564C621BB663B1273773B99_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, void* ___0_pointer, int32_t ___1_length, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		goto IL_0016;
	}

IL_0016:
	{
		int32_t L_0 = ___1_length;
		if ((((int32_t)L_0) >= ((int32_t)0)))
		{
			goto IL_001f;
		}
	}
	{
		ThrowHelper_ThrowArgumentOutOfRangeException_mD7D90276EDCDF9394A8EA635923E3B48BB71BD56(NULL);
	}

IL_001f:
	{
		void* L_1 = ___0_pointer;
		jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225* L_2;
		L_2 = il2cpp_unsafe_as_ref<jvalue_t1756CE401EE222450C9AD0B98CB30E213D4A3225>((uint8_t*)L_1);
		ByReference_1_tE0748F88701C64E729013351521FC3EED28DE1DC L_3;
		memset((&L_3), 0, sizeof(L_3));
		il2cpp_codegen_by_reference_constructor((Il2CppByReference*)(&L_3), L_2);
		__this->____pointer = L_3;
		int32_t L_4 = ___1_length;
		__this->____length = L_4;
		return;
	}
}
// Method Definition Index: 2076
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR int32_t Span_1_get_Length_m9C7E8DECAA7368617C319A866C6A9E960F140BF7_gshared_inline (Span_1_t968992D6F95715A2C7F64EDDA83CD37C8C7CBCD7* __this, const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		int32_t L_0 = __this->____length;
		return L_0;
	}
}
// Method Definition Index: 9547
IL2CPP_MANAGED_FORCE_INLINE IL2CPP_METHOD_ATTR ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* ArraySortHelper_1_get_Default_m9357CBDC001EFB2BA8D065940A5C9FEFED3CC036_gshared_inline (const RuntimeMethod* method) 
{
	//<source_info:<no-source>:1>
	{
		il2cpp_codegen_runtime_class_init_inline(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 9));
		ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28* L_0 = ((ArraySortHelper_1_tEC109463DA70EE9C9096D4971D34412677570D28_StaticFields*)il2cpp_codegen_static_fields_for(il2cpp_rgctx_data_no_init(InitializedTypeInfo(method->klass)->rgctx_data, 9)))->___s_defaultArraySortHelper;
		return L_0;
	}
}
