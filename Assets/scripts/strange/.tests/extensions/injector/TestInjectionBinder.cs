using System;
using NUnit.Framework;
using strange.extensions.injector.api;
using strange.extensions.injector.impl;

namespace strange.unittests
{
	[TestFixture()]
	public class TestinjectionBinder
	{
		IInjectionBinder binder;

		[SetUp]
		public void SetUp()
		{
			binder = new InjectionBinder ();
		}

		[Test]
		public void TestInjectorExists()
		{
			Assert.That (binder.injector != null);
		}

		[Test]
		public void TestGetBindingFlat ()
		{
			binder.Bind<InjectableSuperClass> ().To<InjectableSuperClass> ();
			IInjectionBinding binding = binder.GetBinding<InjectableSuperClass> () as IInjectionBinding;
			Assert.IsNotNull (binding);
		}

		[Test]
		public void TestGetBindingAbstract ()
		{
			binder.Bind<ISimpleInterface> ().To<ClassWithConstructorParameters> ();
			IInjectionBinding binding = binder.GetBinding<ISimpleInterface> () as IInjectionBinding;
			Assert.IsNotNull (binding);
		}

		[Test]
		public void TestGetNamedBinding ()
		{
			binder.Bind<ISimpleInterface> ().To<ClassWithConstructorParameters> ().ToName<MarkerClass>();
			IInjectionBinding binding = binder.GetBinding<ISimpleInterface> (typeof(MarkerClass)) as IInjectionBinding;
			Assert.IsNotNull (binding);
		}

		[Test]
		public void TestGetInstance1()
		{
			binder.Bind<ClassToBeInjected> ().To<ClassToBeInjected> ();

			ClassToBeInjected instance = binder.GetInstance (typeof(ClassToBeInjected)) as ClassToBeInjected;

			Assert.IsNotNull (instance);
			Assert.That (instance is ClassToBeInjected);
		}

		[Test]
		public void TestGetInstance2()
		{
			binder.Bind<ClassToBeInjected> ().To<ClassToBeInjected> ();

			ClassToBeInjected instance = binder.GetInstance<ClassToBeInjected> () as ClassToBeInjected;

			Assert.IsNotNull (instance);
			Assert.That (instance is ClassToBeInjected);
		}

		[Test]
		public void TestGetNamedInstance1()
		{
			binder.Bind<ClassToBeInjected> ().To<ClassToBeInjected> ().ToName<MarkerClass>();

			ClassToBeInjected instance = binder.GetInstance (typeof(ClassToBeInjected), typeof(MarkerClass)) as ClassToBeInjected;

			Assert.IsNotNull (instance);
			Assert.That (instance is ClassToBeInjected);
		}

		[Test]
		public void TestGetNamedInstance2()
		{
			binder.Bind<ClassToBeInjected> ().To<ClassToBeInjected> ().ToName<MarkerClass>();

			ClassToBeInjected instance = binder.GetInstance<ClassToBeInjected> (typeof(MarkerClass)) as ClassToBeInjected;

			Assert.IsNotNull (instance);
			Assert.That (instance is ClassToBeInjected);
		}

		[Test]
		public void TestGetNamedInstance3()
		{
			binder.Bind<ClassToBeInjected> ().To<ClassToBeInjected> ().ToName(SomeEnum.ONE);

			ClassToBeInjected instance = binder.GetInstance (typeof(ClassToBeInjected), SomeEnum.ONE) as ClassToBeInjected;

			Assert.IsNotNull (instance);
			Assert.That (instance is ClassToBeInjected);
		}

		[Test]
		public void TestGetNamedInstance4()
		{
			binder.Bind<ClassToBeInjected> ().To<ClassToBeInjected> ().ToName(SomeEnum.ONE);

			ClassToBeInjected instance = binder.GetInstance<ClassToBeInjected> (SomeEnum.ONE) as ClassToBeInjected;

			Assert.IsNotNull (instance);
			Assert.That (instance is ClassToBeInjected);
		}

		[Test]
		public void TestInjectionErrorFailureToProvideDependency()
		{
			TestDelegate testDelegate = delegate() {
				binder.GetInstance<InjectableSuperClass> ();
			};
			binder.Bind<InjectableSuperClass> ().To<InjectableSuperClass> ();
			InjectionException ex = Assert.Throws<InjectionException> (testDelegate);
			Assert.That (ex.type == InjectionExceptionType.NULL_BINDING);
		}

		[Test]
		public void TestInjectionProvideIntDependency()
		{
			binder.Bind<InjectableSuperClass> ().To<InjectableSuperClass> ();
			binder.Bind<int> ().AsValue (42);
			InjectableSuperClass testValue = binder.GetInstance<InjectableSuperClass> () as InjectableSuperClass;
			Assert.IsNotNull (testValue);
			Assert.That (testValue.intValue == 42);
		}

		[Test]
		public void TestRemoveDependency()
		{
			binder.Bind<InjectableSuperClass> ().To<InjectableSuperClass> ();
			binder.Bind<int> ().AsValue (42);
			InjectableSuperClass testValueBeforeUnbinding = binder.GetInstance<InjectableSuperClass> () as InjectableSuperClass;
			Assert.IsNotNull (testValueBeforeUnbinding);
			Assert.That (testValueBeforeUnbinding.intValue == 42);

			binder.Unbind<int> ();

			TestDelegate testDelegate = delegate() {
				binder.GetInstance<InjectableSuperClass> ();
			};

			InjectionException ex = Assert.Throws<InjectionException> (testDelegate);
			Assert.That (ex.type == InjectionExceptionType.NULL_BINDING);
		}

		[Test]
		public void TestValueAsSingleton()
		{
			GuaranteedUniqueInstances uniqueInstance = new GuaranteedUniqueInstances ();
			binder.Bind<GuaranteedUniqueInstances> ().AsValue (uniqueInstance);
			GuaranteedUniqueInstances instance1 = binder.GetInstance <GuaranteedUniqueInstances> () as GuaranteedUniqueInstances;
			GuaranteedUniqueInstances instance2 = binder.GetInstance <GuaranteedUniqueInstances> () as GuaranteedUniqueInstances;
			Assert.AreEqual (instance1.uid, instance2.uid);
		}

		[Test]
		public void TestPolymorphicBinding()
		{
			binder.Bind<ISimpleInterface> ().Bind<IAnotherSimpleInterface> ().To<PolymorphicClass> ();

			ISimpleInterface callOnce = binder.GetInstance<ISimpleInterface> () as ISimpleInterface;
			Assert.NotNull (callOnce);
			Assert.IsInstanceOf<PolymorphicClass> (callOnce);

			IAnotherSimpleInterface callAgain = binder.GetInstance<IAnotherSimpleInterface> () as IAnotherSimpleInterface;
			Assert.NotNull (callAgain);
			Assert.IsInstanceOf<PolymorphicClass> (callAgain);
		}
	}
}
