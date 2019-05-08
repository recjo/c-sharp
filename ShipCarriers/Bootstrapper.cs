	//other code removed for brevity
	unity.RegisterType<ICarrierServiceFactory, CarrierServiceFactory>(new HierarchicalLifetimeManager());
	unity.RegisterType<ICarrierService, UpsService>(Enum.GetName(typeof(CarrierType), CarrierType.UPS));
	unity.RegisterType<IUpsClient, UpsClient>(new HierarchicalLifetimeManager());
	unity.RegisterType<ICarrierService, FedExService>(Enum.GetName(typeof(CarrierType), CarrierType.FedEx));
	unity.RegisterType<IFedExClient, FedExClient>(new HierarchicalLifetimeManager());