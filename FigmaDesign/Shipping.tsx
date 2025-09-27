import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Progress } from "./ui/progress";
import { Dialog, DialogContent, DialogDescription, DialogHeader, DialogTitle, DialogTrigger } from "./ui/dialog";
import { Textarea } from "./ui/textarea";
import { Checkbox } from "./ui/checkbox";
import { 
  Truck, 
  Package, 
  MapPin, 
  Weight, 
  Clock, 
  Calendar,
  Plus,
  Search,
  CheckCircle2,
  AlertCircle,
  Navigation,
  User,
  Phone,
  FileText,
  Edit,
  X,
  ArrowRight,
  Route,
  Fuel,
  DollarSign,
  Calculator,
  Play,
  Pause,
  StopCircle,
  Locate,
  MessageSquare,
  Camera,
  QrCode,
  PrinterIcon,
  Building,
  Layers,
  Target,
  Users,
  Timer,
  Info
} from "lucide-react";
import { StatusChip } from "./StatusChip";

interface LoadItem {
  id: string;
  pickingListId: string;
  customerName: string;
  orderNumber: string;
  destination: string;
  weight: number;
  pieces: number;
  dimensions: {
    length: number;
    width: number;
    height: number;
  };
  specialInstructions?: string;
  priority: "low" | "normal" | "high" | "urgent";
  requiredDeliveryDate: string;
  customerContact: string;
  customerPhone: string;
  loadingRequirements?: string;
  unloadingRequirements?: string;
  deliveryStatus?: "pending" | "in-progress" | "delivered" | "failed";
  deliveryTime?: string;
  signedBy?: string;
  deliveryNotes?: string;
  proofOfDelivery?: string[];
}

interface AvailableOrder {
  id: string;
  pickingListId: string;
  customerName: string;
  orderNumber: string;
  destination: string;
  city: string;
  state: string;
  zipCode: string;
  province?: string;
  weight: number;
  pieces: number;
  dimensions: {
    length: number;
    width: number;
    height: number;
  };
  priority: "low" | "normal" | "high" | "urgent";
  requiredDeliveryDate: string;
  readyDate: string;
  customerContact: string;
  customerPhone: string;
  specialInstructions?: string;
  loadingRequirements?: string;
  unloadingRequirements?: string;
  pickingStatus: "picked" | "packed" | "ready";
  value: number;
  distance: number; // from warehouse
  deliveryRegion: "local" | "out-of-town" | "island-pool" | "okanagan-pool" | "customer-pickup";
  regionZone?: string; // specific zone within region
  isPoolDelivery?: boolean;
  poolRoute?: string;
  customerPickupScheduled?: string;
  ferryRequirements?: {
    required: boolean;
    route?: string;
    schedule?: string;
    additionalCost?: number;
  };
}

interface Load {
  id: string;
  truckNumber: string;
  driverName: string;
  driverPhone: string;
  driverEmail: string;
  trailerType: "flatbed" | "enclosed" | "step-deck" | "dry-van" | "refrigerated";
  maxWeight: number;
  maxDimensions: {
    length: number;
    width: number;
    height: number;
  };
  status: "planning" | "loading" | "loaded" | "dispatched" | "in-transit" | "delivered" | "exception";
  priority: "low" | "normal" | "high" | "urgent";
  scheduledPickup: string;
  estimatedDelivery?: string;
  actualPickup?: string;
  actualDelivery?: string;
  route: string[];
  items: LoadItem[];
  currentWeight: number;
  utilizationPercentage: number;
  notes?: string;
  createdBy: string;
  createdAt: string;
  dispatchedAt?: string;
  dispatchedBy?: string;
  totalDistance: number;
  estimatedFuelCost: number;
  totalValue: number;
  currentLocation?: {
    lat: number;
    lng: number;
    address: string;
    timestamp: string;
  };
  trackingUpdates: TrackingUpdate[];
  exceptions?: LoadException[];
  deliveryRegion: "local" | "out-of-town" | "island-pool" | "okanagan-pool" | "customer-pickup";
  regionRoute?: RegionalRoute;
  poolInfo?: PoolTruckInfo;
  ferrySchedule?: FerrySchedule[];
  customerPickupWindow?: {
    start: string;
    end: string;
    location: string;
    contact: string;
    instructions?: string;
  };
}

interface TrackingUpdate {
  id: string;
  timestamp: string;
  location: {
    lat: number;
    lng: number;
    address: string;
  };
  status: string;
  notes?: string;
  updatedBy: string;
}

interface LoadException {
  id: string;
  type: "delay" | "damage" | "weather" | "breakdown" | "customer-issue" | "other";
  severity: "low" | "medium" | "high" | "critical";
  description: string;
  reportedAt: string;
  reportedBy: string;
  resolved: boolean;
  resolvedAt?: string;
  resolution?: string;
}

interface RegionalRoute {
  region: string;
  subRegions: string[];
  estimatedDays: number;
  ferryRequired: boolean;
  poolOptimized: boolean;
  routePattern: "fixed" | "flexible" | "on-demand";
}

interface PoolTruckInfo {
  poolType: "island" | "okanagan" | "multi-town";
  poolRoute: string;
  frequency: "daily" | "weekly" | "bi-weekly" | "on-demand";
  nextDeparture: string;
  capacity: {
    weight: number;
    volume: number;
    currentUtilization: number;
  };
  consolidationDeadline: string;
  coordinator: string;
  coordinatorPhone: string;
}

interface FerrySchedule {
  route: string;
  departure: string;
  arrival: string;
  cost: number;
  bookingRequired: boolean;
  cutoffTime: string;
}

interface DeliveryRegion {
  id: string;
  name: string;
  type: "local" | "out-of-town" | "island-pool" | "okanagan-pool" | "customer-pickup";
  description: string;
  coverage: string[];
  characteristics: {
    averageDistance: number;
    deliveryFrequency: string;
    requiresPooling: boolean;
    ferryDependent: boolean;
    seasonalAccess: boolean;
  };
  operationalInfo: {
    coordinator: string;
    phone: string;
    email: string;
    workingHours: string;
    specialRequirements: string[];
  };
  metrics: {
    activeOrders: number;
    pendingPickups: number;
    averageDeliveryTime: string;
    utilizationRate: number;
    costPerDelivery: number;
  };
}

interface Truck {
  id: string;
  number: string;
  type: "flatbed" | "enclosed" | "step-deck" | "dry-van" | "refrigerated";
  maxWeight: number;
  maxDimensions: {
    length: number;
    width: number;
    height: number;
  };
  status: "available" | "assigned" | "maintenance" | "out-of-service";
  currentDriver?: string;
  location?: string;
  fuelEfficiency: number; // mpg
  assignedRegion?: string;
  poolTruck?: boolean;
  ferryCapable?: boolean;
}

const truckTypes = ["flatbed", "enclosed", "step-deck", "dry-van", "refrigerated"];
const deliveryRegions = ["local", "out-of-town", "island-pool", "okanagan-pool", "customer-pickup"];

const regionalData: DeliveryRegion[] = [
  {
    id: "local",
    name: "Local Delivery",
    type: "local",
    description: "Same-day and next-day deliveries within metro area",
    coverage: ["Vancouver", "Burnaby", "Richmond", "Surrey", "Coquitlam", "North Vancouver"],
    characteristics: {
      averageDistance: 25,
      deliveryFrequency: "Daily",
      requiresPooling: false,
      ferryDependent: false,
      seasonalAccess: false
    },
    operationalInfo: {
      coordinator: "Sarah Chen",
      phone: "(604) 555-0123",
      email: "sarah.chen@metalflow.com",
      workingHours: "6:00 AM - 6:00 PM",
      specialRequirements: ["Traffic restrictions in downtown core", "Dock height limitations"]
    },
    metrics: {
      activeOrders: 23,
      pendingPickups: 0,
      averageDeliveryTime: "4-8 hours",
      utilizationRate: 85,
      costPerDelivery: 125
    }
  },
  {
    id: "out-of-town",
    name: "Multi Out of Town Lanes",
    type: "out-of-town",
    description: "Regional deliveries to multiple towns and cities",
    coverage: ["Calgary", "Edmonton", "Saskatoon", "Regina", "Winnipeg", "Kelowna"],
    characteristics: {
      averageDistance: 450,
      deliveryFrequency: "2-3 times weekly",
      requiresPooling: true,
      ferryDependent: false,
      seasonalAccess: true
    },
    operationalInfo: {
      coordinator: "Mike Rodriguez",
      phone: "(604) 555-0456",
      email: "mike.rodriguez@metalflow.com",
      workingHours: "24/7 Operations",
      specialRequirements: ["Winter driving conditions", "Cross-border documentation", "Fuel planning"]
    },
    metrics: {
      activeOrders: 18,
      pendingPickups: 3,
      averageDeliveryTime: "2-4 days",
      utilizationRate: 72,
      costPerDelivery: 850
    }
  },
  {
    id: "island-pool",
    name: "Island Pool Trucks",
    type: "island-pool",
    description: "Consolidated ferry-dependent deliveries to Vancouver Island",
    coverage: ["Victoria", "Nanaimo", "Duncan", "Courtenay", "Campbell River", "Port Alberni"],
    characteristics: {
      averageDistance: 120,
      deliveryFrequency: "Twice weekly",
      requiresPooling: true,
      ferryDependent: true,
      seasonalAccess: false
    },
    operationalInfo: {
      coordinator: "Jennifer Wilson",
      phone: "(604) 555-0789",
      email: "jennifer.wilson@metalflow.com",
      workingHours: "5:00 AM - 8:00 PM",
      specialRequirements: ["Ferry reservations", "Tidal schedules", "Island weight restrictions"]
    },
    metrics: {
      activeOrders: 12,
      pendingPickups: 8,
      averageDeliveryTime: "3-5 days",
      utilizationRate: 68,
      costPerDelivery: 320
    }
  },
  {
    id: "okanagan-pool",
    name: "Okanagan Pool Trucks",
    type: "okanagan-pool",
    description: "Pooled deliveries to Okanagan Valley region",
    coverage: ["Kelowna", "Vernon", "Penticton", "Kamloops", "Salmon Arm", "Oliver"],
    characteristics: {
      averageDistance: 380,
      deliveryFrequency: "Weekly",
      requiresPooling: true,
      ferryDependent: false,
      seasonalAccess: true
    },
    operationalInfo: {
      coordinator: "Carlos Martinez",
      phone: "(604) 555-0321",
      email: "carlos.martinez@metalflow.com",
      workingHours: "6:00 AM - 6:00 PM",
      specialRequirements: ["Mountain pass conditions", "Seasonal road closures", "Rural access planning"]
    },
    metrics: {
      activeOrders: 8,
      pendingPickups: 5,
      averageDeliveryTime: "4-7 days",
      utilizationRate: 58,
      costPerDelivery: 420
    }
  },
  {
    id: "customer-pickup",
    name: "Customer Pickup",
    type: "customer-pickup",
    description: "Customer self-pickup coordination and scheduling",
    coverage: ["Warehouse Dock A", "Warehouse Dock B", "Will Call Area"],
    characteristics: {
      averageDistance: 0,
      deliveryFrequency: "On-demand",
      requiresPooling: false,
      ferryDependent: false,
      seasonalAccess: false
    },
    operationalInfo: {
      coordinator: "Lisa Thompson",
      phone: "(604) 555-0654",
      email: "lisa.thompson@metalflow.com",
      workingHours: "7:00 AM - 5:00 PM",
      specialRequirements: ["Pickup appointments", "Loading equipment", "Documentation verification"]
    },
    metrics: {
      activeOrders: 15,
      pendingPickups: 9,
      averageDeliveryTime: "Same day",
      utilizationRate: 95,
      costPerDelivery: 0
    }
  }
];

const availableTrucks: Truck[] = [
  {
    id: "T-401",
    number: "T-401",
    type: "flatbed",
    maxWeight: 48000,
    maxDimensions: { length: 48, width: 8.5, height: 8.5 },
    status: "assigned",
    currentDriver: "Mike Rodriguez",
    location: "Loading Dock A",
    fuelEfficiency: 6.5,
    assignedRegion: "out-of-town",
    poolTruck: true,
    ferryCapable: false
  },
  {
    id: "T-205",
    number: "T-205", 
    type: "enclosed",
    maxWeight: 44000,
    maxDimensions: { length: 45, width: 8, height: 9 },
    status: "available",
    location: "Yard",
    fuelEfficiency: 7.2,
    assignedRegion: "local",
    poolTruck: false,
    ferryCapable: false
  },
  {
    id: "T-312",
    number: "T-312",
    type: "step-deck",
    maxWeight: 52000,
    maxDimensions: { length: 50, width: 8.5, height: 11.5 },
    status: "assigned",
    currentDriver: "Jennifer Wilson",
    location: "On Route",
    fuelEfficiency: 6.0,
    assignedRegion: "island-pool",
    poolTruck: true,
    ferryCapable: true
  },
  {
    id: "T-156",
    number: "T-156",
    type: "dry-van",
    maxWeight: 42000,
    maxDimensions: { length: 40, width: 8, height: 9 },
    status: "available",
    location: "Maintenance Bay",
    fuelEfficiency: 7.8,
    assignedRegion: "local",
    poolTruck: false,
    ferryCapable: false
  },
  {
    id: "T-098",
    number: "T-098",
    type: "refrigerated",
    maxWeight: 40000,
    maxDimensions: { length: 40, width: 8, height: 9 },
    status: "available",
    location: "Yard",
    fuelEfficiency: 6.8,
    assignedRegion: "okanagan-pool",
    poolTruck: true,
    ferryCapable: false
  },
  {
    id: "T-789",
    number: "T-789",
    type: "flatbed",
    maxWeight: 46000,
    maxDimensions: { length: 48, width: 8.5, height: 8.5 },
    status: "available",
    location: "Dock B",
    fuelEfficiency: 6.8,
    assignedRegion: "island-pool",
    poolTruck: true,
    ferryCapable: true
  },
  {
    id: "T-432",
    number: "T-432",
    type: "dry-van",
    maxWeight: 44000,
    maxDimensions: { length: 45, width: 8, height: 9 },
    status: "available",
    location: "Yard",
    fuelEfficiency: 7.5,
    assignedRegion: "out-of-town",
    poolTruck: true,
    ferryCapable: false
  }
];

const availableOrders: AvailableOrder[] = [
  {
    id: "ORD-2024-0094",
    pickingListId: "PL-2024-0094",
    customerName: "Victoria Steel Works",
    orderNumber: "VSW-5567",
    destination: "2340 Industrial Way, Victoria, BC V8Z 3Y8",
    city: "Victoria",
    state: "BC",
    province: "BC",
    zipCode: "V8Z3Y8",
    weight: 6200,
    pieces: 35,
    dimensions: { length: 10, width: 4, height: 2 },
    priority: "high",
    requiredDeliveryDate: "2024-01-22",
    readyDate: "2024-01-18",
    customerContact: "Jim Peterson",
    customerPhone: "(250) 555-0178",
    specialInstructions: "Ferry booking required - coordinate with dispatch",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has crane",
    pickingStatus: "ready",
    value: 28500,
    distance: 115,
    deliveryRegion: "island-pool",
    regionZone: "South Island",
    isPoolDelivery: true,
    poolRoute: "VI-SOUTH-01",
    ferryRequirements: {
      required: true,
      route: "Tsawwassen-Swartz Bay",
      schedule: "Multiple daily sailings",
      additionalCost: 185
    }
  },
  {
    id: "ORD-2024-0095",
    pickingListId: "PL-2024-0095",
    customerName: "Calgary Steel Dynamics",
    orderNumber: "CSD-7789",
    destination: "890 Steel Mill Rd, Calgary, AB T2E 6J8",
    city: "Calgary",
    state: "AB",
    province: "AB",
    zipCode: "T2E6J8", 
    weight: 12400,
    pieces: 20,
    dimensions: { length: 20, width: 8, height: 3 },
    priority: "urgent",
    requiredDeliveryDate: "2024-01-20",
    readyDate: "2024-01-17",
    customerContact: "Maria Gonzalez",
    customerPhone: "(403) 555-0234",
    specialInstructions: "Cross-border documentation required",
    loadingRequirements: "Overhead crane needed",
    unloadingRequirements: "Customer unloads",
    pickingStatus: "packed",
    value: 45200,
    distance: 295,
    deliveryRegion: "out-of-town",
    regionZone: "Alberta Corridor",
    isPoolDelivery: true,
    poolRoute: "AB-NORTH-01"
  },
  {
    id: "ORD-2024-0096", 
    pickingListId: "PL-2024-0096",
    customerName: "Richmond Auto Parts",
    orderNumber: "RAP-3321",
    destination: "1500 Auto Parts Way, Richmond, BC V7A 4Z9",
    city: "Richmond",
    state: "BC",
    province: "BC",
    zipCode: "V7A4Z9",
    weight: 3850,
    pieces: 75,
    dimensions: { length: 6, width: 3, height: 1.5 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-19",
    readyDate: "2024-01-18", 
    customerContact: "Robert Kim",
    customerPhone: "(604) 555-0445",
    specialInstructions: "Morning delivery preferred",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer forklift available",
    pickingStatus: "ready",
    value: 18750,
    distance: 25,
    deliveryRegion: "local",
    regionZone: "Metro Vancouver"
  },
  {
    id: "ORD-2024-0097",
    pickingListId: "PL-2024-0097", 
    customerName: "Kelowna Construction Supply",
    orderNumber: "KCS-8854",
    destination: "780 Builder Ave, Kelowna, BC V1X 7G5",
    city: "Kelowna",
    state: "BC",
    province: "BC",
    zipCode: "V1X7G5",
    weight: 8900,
    pieces: 45,
    dimensions: { length: 12, width: 6, height: 2.5 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-23",
    readyDate: "2024-01-19",
    customerContact: "Lisa Chen",
    customerPhone: "(250) 555-0667",
    specialInstructions: "Call 30 minutes before arrival - mountain pass conditions may apply",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Dock level unloading",
    pickingStatus: "picked",
    value: 32400,
    distance: 380,
    deliveryRegion: "okanagan-pool",
    regionZone: "Central Okanagan",
    isPoolDelivery: true,
    poolRoute: "OK-CENTRAL-01"
  },
  {
    id: "ORD-2024-0098",
    pickingListId: "PL-2024-0098",
    customerName: "Pacific Fabrication Works",
    orderNumber: "PFW-4412",
    destination: "Customer Pickup - Will Call Area",
    city: "Vancouver", 
    state: "BC",
    province: "BC",
    zipCode: "V6B1A1",
    weight: 15600,
    pieces: 12,
    dimensions: { length: 25, width: 8, height: 4 },
    priority: "high",
    requiredDeliveryDate: "2024-01-19",
    readyDate: "2024-01-18",
    customerContact: "David Johnson",
    customerPhone: "(604) 555-0889",
    specialInstructions: "Customer pickup - heavy lift equipment needed for loading",
    loadingRequirements: "Overhead crane required",
    unloadingRequirements: "Customer provides transport",
    pickingStatus: "ready",
    value: 62300,
    distance: 0,
    deliveryRegion: "customer-pickup",
    regionZone: "Warehouse",
    customerPickupScheduled: "2024-01-19T10:00:00"
  },
  {
    id: "ORD-2024-0099",
    pickingListId: "PL-2024-0099",
    customerName: "Nanaimo Builders",
    orderNumber: "NB-9987",
    destination: "450 Metro Center Dr, Nanaimo, BC V9S 2E6",
    city: "Nanaimo",
    state: "BC", 
    province: "BC",
    zipCode: "V9S2E6",
    weight: 5200,
    pieces: 60,
    dimensions: { length: 8, width: 4, height: 1 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-22",
    readyDate: "2024-01-19",
    customerContact: "Amanda Foster",
    customerPhone: "(250) 555-0334",
    specialInstructions: "Ferry dependent - coordinate with island pool schedule",
    loadingRequirements: "Hand truck needed",
    unloadingRequirements: "Ground level delivery",
    pickingStatus: "ready",
    value: 21800,
    distance: 95,
    deliveryRegion: "island-pool",
    regionZone: "Mid Island",
    isPoolDelivery: true,
    poolRoute: "VI-CENTRAL-01",
    ferryRequirements: {
      required: true,
      route: "Horseshoe Bay-Departure Bay",
      schedule: "Every 2 hours",
      additionalCost: 165
    }
  }
];

const sampleLoads: Load[] = [
  {
    id: "LD-2024-0067",
    truckNumber: "T-401",
    driverName: "Mike Rodriguez",
    driverPhone: "(555) 123-4567",
    driverEmail: "mrodriguez@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 48000,
    maxDimensions: { length: 48, width: 8.5, height: 8.5 },
    status: "in-transit",
    priority: "high",
    scheduledPickup: "2024-01-18T08:00:00",
    estimatedDelivery: "2024-01-18T16:30:00",
    actualPickup: "2024-01-18T08:15:00",
    dispatchedAt: "2024-01-18T08:30:00",
    dispatchedBy: "Sarah Chen",
    route: ["Precision Manufacturing", "ABC Construction", "Industrial Fabricators"],
    currentWeight: 11400,
    utilizationPercentage: 75,
    totalDistance: 485,
    estimatedFuelCost: 145,
    totalValue: 92450,
    notes: "Multi-stop delivery - call customers 30 min before arrival",
    createdBy: "Sarah Chen",
    createdAt: "2024-01-17T14:30:00Z",
    currentLocation: {
      lat: 49.2827,
      lng: -123.1207,
      address: "Highway 1 near Vancouver, BC",
      timestamp: "2024-01-18T11:30:00Z"
    },
    deliveryRegion: "island-pool",
    regionRoute: {
      region: "Vancouver Island",
      subRegions: ["Victoria", "Nanaimo"],
      estimatedDays: 3,
      ferryRequired: true,
      poolOptimized: true,
      routePattern: "fixed"
    },
    poolInfo: {
      poolType: "island",
      poolRoute: "VI-SOUTH-01",
      frequency: "daily",
      nextDeparture: "2024-01-18T14:00:00Z",
      capacity: {
        weight: 48000,
        volume: 2400,
        currentUtilization: 75
      },
      consolidationDeadline: "2024-01-18T13:00:00Z",
      coordinator: "Jennifer Wilson",
      coordinatorPhone: "(604) 555-0789"
    },
    ferrySchedule: [
      {
        route: "Tsawwassen-Swartz Bay",
        departure: "2024-01-18T14:00:00Z",
        arrival: "2024-01-18T15:35:00Z",
        cost: 185,
        bookingRequired: true,
        cutoffTime: "2024-01-18T13:30:00Z"
      }
    ],
    trackingUpdates: [
      {
        id: "TU-001",
        timestamp: "2024-01-18T08:15:00Z",
        location: { lat: 41.8781, lng: -87.6298, address: "MetalFlow Warehouse, Chicago, IL" },
        status: "Departed warehouse",
        updatedBy: "Mike Rodriguez"
      },
      {
        id: "TU-002", 
        timestamp: "2024-01-18T11:30:00Z",
        location: { lat: 41.8781, lng: -87.6298, address: "I-90 E near Chicago, IL" },
        status: "En route to first delivery",
        notes: "Traffic moving well, on schedule",
        updatedBy: "Mike Rodriguez"
      }
    ],
    items: [
      {
        id: "LI-067-001",
        pickingListId: "PL-2024-0089",
        customerName: "Precision Manufacturing",
        orderNumber: "PM-8891",
        destination: "123 Industrial Way, Chicago, IL 60632",
        weight: 5700,
        pieces: 25,
        dimensions: { length: 8, width: 4, height: 1 },
        priority: "high",
        requiredDeliveryDate: "2024-01-18",
        customerContact: "Tom Wilson",
        customerPhone: "(312) 555-0156",
        specialInstructions: "Fragile - handle with care",
        unloadingRequirements: "Dock level delivery",
        deliveryStatus: "pending"
      },
      {
        id: "LI-067-002", 
        pickingListId: "PL-2024-0090",
        customerName: "ABC Construction",
        orderNumber: "ABC-4412",
        destination: "456 Builder Blvd, Milwaukee, WI 53202",
        weight: 3720,
        pieces: 50,
        dimensions: { length: 10, width: 5, height: 0.5 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-18", 
        customerContact: "Steve Johnson",
        customerPhone: "(414) 555-0298",
        unloadingRequirements: "Ground level delivery",
        deliveryStatus: "pending"
      },
      {
        id: "LI-067-003",
        pickingListId: "PL-2024-0091", 
        customerName: "Industrial Fabricators",
        orderNumber: "IF-9923",
        destination: "789 Factory St, Detroit, MI 48201",
        weight: 1980,
        pieces: 15,
        dimensions: { length: 10, width: 3, height: 2 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-18",
        customerContact: "Lisa Martinez",
        customerPhone: "(313) 555-0445",
        unloadingRequirements: "Customer crane available",
        deliveryStatus: "pending"
      }
    ]
  },
  {
    id: "LD-2024-0068",
    truckNumber: "T-205",
    driverName: "Carlos Martinez",
    driverPhone: "(555) 987-6543",
    driverEmail: "cmartinez@metalflow.com",
    trailerType: "enclosed",
    maxWeight: 44000,
    maxDimensions: { length: 45, width: 8, height: 9 },
    status: "planning",
    priority: "normal",
    scheduledPickup: "2024-01-19T07:00:00",
    route: ["Metro Steel Works", "City Fabrication"],
    currentWeight: 8940,
    utilizationPercentage: 45,
    totalDistance: 285,
    estimatedFuelCost: 95,
    totalValue: 58750,
    createdBy: "John Miller",
    createdAt: "2024-01-17T16:15:00Z",
    trackingUpdates: [],
    deliveryRegion: "okanagan-pool",
    regionRoute: {
      region: "Okanagan Valley",
      subRegions: ["Kelowna", "Vernon"],
      estimatedDays: 4,
      ferryRequired: false,
      poolOptimized: true,
      routePattern: "flexible"
    },
    poolInfo: {
      poolType: "okanagan",
      poolRoute: "OK-CENTRAL-01",
      frequency: "weekly",
      nextDeparture: "2024-01-19T06:00:00Z",
      capacity: {
        weight: 44000,
        volume: 2200,
        currentUtilization: 45
      },
      consolidationDeadline: "2024-01-19T05:00:00Z",
      coordinator: "Carlos Martinez",
      coordinatorPhone: "(604) 555-0321"
    },
    items: [
      {
        id: "LI-068-001",
        pickingListId: "PL-2024-0092",
        customerName: "Metro Steel Works", 
        orderNumber: "MSW-3345",
        destination: "321 Metro Ave, Springfield, IL 62701",
        weight: 4420,
        pieces: 30,
        dimensions: { length: 8, width: 4, height: 1.5 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-19",
        customerContact: "Mark Stevens",
        customerPhone: "(217) 555-0234",
        unloadingRequirements: "Forklift available",
        deliveryStatus: "pending"
      },
      {
        id: "LI-068-002",
        pickingListId: "PL-2024-0093",
        customerName: "City Fabrication",
        orderNumber: "CF-7788",
        destination: "654 City Plaza, Rockford, IL 61101", 
        weight: 4520,
        pieces: 25,
        dimensions: { length: 12, width: 3, height: 2 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-19",
        customerContact: "Jennifer Lee",
        customerPhone: "(815) 555-0667",
        unloadingRequirements: "Loading dock available",
        deliveryStatus: "pending"
      }
    ]
  },
  {
    id: "LD-2024-0069",
    truckNumber: "T-312",
    driverName: "Jennifer Wilson",
    driverPhone: "(555) 555-0123",
    driverEmail: "jwilson@metalflow.com",
    trailerType: "step-deck",
    maxWeight: 52000,
    maxDimensions: { length: 50, width: 8.5, height: 11.5 },
    status: "delivered",
    priority: "urgent",
    scheduledPickup: "2024-01-16T06:00:00",
    estimatedDelivery: "2024-01-16T18:00:00",
    actualPickup: "2024-01-16T06:15:00",
    actualDelivery: "2024-01-16T17:45:00",
    dispatchedAt: "2024-01-16T06:30:00",
    dispatchedBy: "Maria Santos",
    route: ["Heavy Industries Corp"],
    currentWeight: 31200,
    utilizationPercentage: 95,
    totalDistance: 125,
    estimatedFuelCost: 52,
    totalValue: 125000,
    notes: "Oversized load - special permits obtained",
    createdBy: "Maria Santos",
    createdAt: "2024-01-15T11:00:00Z",
    deliveryRegion: "out-of-town",
    regionRoute: {
      region: "Alberta Corridor",
      subRegions: ["Calgary"],
      estimatedDays: 2,
      ferryRequired: false,
      poolOptimized: false,
      routePattern: "fixed"
    },
    trackingUpdates: [
      {
        id: "TU-003",
        timestamp: "2024-01-16T06:15:00Z",
        location: { lat: 41.8781, lng: -87.6298, address: "MetalFlow Warehouse, Chicago, IL" },
        status: "Departed warehouse",
        updatedBy: "Jennifer Wilson"
      },
      {
        id: "TU-004",
        timestamp: "2024-01-16T17:45:00Z",
        location: { lat: 41.5868, lng: -87.3467, address: "Heavy Industries Corp, Gary, IN" },
        status: "Delivered",
        notes: "Delivery completed successfully",
        updatedBy: "Jennifer Wilson"
      }
    ],
    items: [
      {
        id: "LI-069-001",
        pickingListId: "PL-2024-0087",
        customerName: "Heavy Industries Corp",
        orderNumber: "HIC-9988",
        destination: "999 Heavy Industry Dr, Gary, IN 46402",
        weight: 31200,
        pieces: 5,
        dimensions: { length: 40, width: 8, height: 10 },
        priority: "urgent",
        requiredDeliveryDate: "2024-01-16",
        customerContact: "Robert Chen",
        customerPhone: "(219) 555-0789",
        specialInstructions: "Requires crane for unloading - coordinate with customer",
        unloadingRequirements: "Customer crane available",
        deliveryStatus: "delivered",
        deliveryTime: "2024-01-16T17:45:00Z",
        signedBy: "Robert Chen",
        deliveryNotes: "Unloaded successfully with customer crane"
      }
    ]
  }
];

const getStatusColor = (status: string) => {
  switch (status) {
    case "planning": return "bg-gray-100 text-gray-800 border-gray-200";
    case "loading": return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case "loaded": return "bg-blue-100 text-blue-800 border-blue-200";
    case "dispatched": return "bg-purple-100 text-purple-800 border-purple-200";
    case "in-transit": return "bg-orange-100 text-orange-800 border-orange-200";
    case "delivered": return "bg-green-100 text-green-800 border-green-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

const getPriorityColor = (priority: string) => {
  switch (priority) {
    case "urgent": return "bg-red-100 text-red-800 border-red-200";
    case "high": return "bg-orange-100 text-orange-800 border-orange-200";
    case "normal": return "bg-blue-100 text-blue-800 border-blue-200";
    case "low": return "bg-gray-100 text-gray-800 border-gray-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Shipping() {
  const [activeTab, setActiveTab] = useState("regional");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [selectedTrailerType, setSelectedTrailerType] = useState("all");
  const [selectedRegion, setSelectedRegion] = useState("all");
  const [showLoadBuilder, setShowLoadBuilder] = useState(false);
  const [showDispatchDialog, setShowDispatchDialog] = useState(false);
  const [showTrackingDialog, setShowTrackingDialog] = useState(false);
  const [selectedLoad, setSelectedLoad] = useState<Load | null>(null);
  const [newLoad, setNewLoad] = useState({
    truckNumber: "",
    driverName: "",
    driverPhone: "",
    driverEmail: "",
    trailerType: "flatbed" as const,
    scheduledPickup: "",
    notes: "",
    isPartialLoad: false,
    maxUtilization: 100
  });
  const [selectedOrders, setSelectedOrders] = useState<string[]>([]);
  const [loadPlannerFilters, setLoadPlannerFilters] = useState({
    priority: "all",
    destination: "all",
    status: "all"
  });

  const filteredLoads = sampleLoads.filter(load => {
    const matchesSearch = load.id.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         load.truckNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         load.driverName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         load.items.some(item => 
                           item.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                           item.orderNumber.toLowerCase().includes(searchTerm.toLowerCase())
                         );
    const matchesStatus = selectedStatus === "all" || load.status === selectedStatus;
    const matchesTrailerType = selectedTrailerType === "all" || load.trailerType === selectedTrailerType;
    return matchesSearch && matchesStatus && matchesTrailerType;
  });

  return (
    <div className="space-y-6">
      <div>
        <h1>Loads & Shipping</h1>
        <p className="text-muted-foreground">Manage truck loads and shipping operations</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
        <TabsList className="grid w-full grid-cols-7">
          <TabsTrigger value="regional" className="flex items-center gap-2">
            <MapPin className="h-4 w-4" />
            Regional Overview
          </TabsTrigger>
          <TabsTrigger value="loads" className="flex items-center gap-2">
            <Truck className="h-4 w-4" />
            Load Management
          </TabsTrigger>
          <TabsTrigger value="planning" className="flex items-center gap-2">
            <Calculator className="h-4 w-4" />
            Load Planning
          </TabsTrigger>
          <TabsTrigger value="pools" className="flex items-center gap-2">
            <Layers className="h-4 w-4" />
            Pool Management
          </TabsTrigger>
          <TabsTrigger value="execution" className="flex items-center gap-2">
            <Play className="h-4 w-4" />
            Shipping Execution
          </TabsTrigger>
          <TabsTrigger value="tracking" className="flex items-center gap-2">
            <Locate className="h-4 w-4" />
            Live Tracking
          </TabsTrigger>
          <TabsTrigger value="analytics" className="flex items-center gap-2">
            <Target className="h-4 w-4" />
            Analytics
          </TabsTrigger>
        </TabsList>

        {/* Controls */}
        <div className="flex flex-wrap gap-4 items-center justify-between">
          <div className="flex gap-2 items-center">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search loads..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-64"
              />
            </div>
            <Select value={selectedStatus} onValueChange={setSelectedStatus}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="planning">Planning</SelectItem>
                <SelectItem value="loading">Loading</SelectItem>
                <SelectItem value="loaded">Loaded</SelectItem>
                <SelectItem value="dispatched">Dispatched</SelectItem>
                <SelectItem value="in-transit">In Transit</SelectItem>
                <SelectItem value="delivered">Delivered</SelectItem>
              </SelectContent>
            </Select>
            <Select value={selectedTrailerType} onValueChange={setSelectedTrailerType}>
              <SelectTrigger className="w-36">
                <SelectValue placeholder="Trailer type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                {truckTypes.map(type => (
                  <SelectItem key={type} value={type} className="capitalize">
                    {type.replace('-', ' ')}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={selectedRegion} onValueChange={setSelectedRegion}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Delivery region" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Regions</SelectItem>
                <SelectItem value="local">Local</SelectItem>
                <SelectItem value="out-of-town">Out of Town</SelectItem>
                <SelectItem value="island-pool">Island Pool</SelectItem>
                <SelectItem value="okanagan-pool">Okanagan Pool</SelectItem>
                <SelectItem value="customer-pickup">Customer Pickup</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Load
          </Button>
        </div>

        <TabsContent value="regional" className="space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-xl font-semibold">Regional Delivery Management</h2>
              <p className="text-muted-foreground">Manage deliveries across different geographical regions and service types</p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm">
                <Route className="h-4 w-4 mr-2" />
                Optimize Routes
              </Button>
              <Button variant="outline" size="sm">
                <Calendar className="h-4 w-4 mr-2" />
                Schedule Pools
              </Button>
            </div>
          </div>

          {/* Regional Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-5 gap-4">
            {regionalData.map(region => (
              <Card key={region.id} className={`cursor-pointer transition-all hover:shadow-md ${
                region.type === 'local' ? 'border-green-200 bg-green-50' :
                region.type === 'out-of-town' ? 'border-blue-200 bg-blue-50' :
                region.type === 'island-pool' ? 'border-purple-200 bg-purple-50' :
                region.type === 'okanagan-pool' ? 'border-orange-200 bg-orange-50' :
                'border-gray-200 bg-gray-50'
              }`}>
                <CardContent className="pt-4">
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <div className={`p-2 rounded-full ${
                        region.type === 'local' ? 'bg-green-100' :
                        region.type === 'out-of-town' ? 'bg-blue-100' :
                        region.type === 'island-pool' ? 'bg-purple-100' :
                        region.type === 'okanagan-pool' ? 'bg-orange-100' :
                        'bg-gray-100'
                      }`}>
                        {region.type === 'local' ? <Building className="h-4 w-4 text-green-600" /> :
                         region.type === 'out-of-town' ? <Route className="h-4 w-4 text-blue-600" /> :
                         region.type === 'island-pool' ? <Navigation className="h-4 w-4 text-purple-600" /> :
                         region.type === 'okanagan-pool' ? <MapPin className="h-4 w-4 text-orange-600" /> :
                         <Users className="h-4 w-4 text-gray-600" />}
                      </div>
                      <Badge variant="outline" className="text-xs">
                        {region.metrics.utilizationRate}%
                      </Badge>
                    </div>
                    
                    <div>
                      <h3 className="font-medium text-sm">{region.name}</h3>
                      <p className="text-xs text-muted-foreground">{region.description}</p>
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-xs">
                        <span className="text-muted-foreground">Active Orders:</span>
                        <span className="font-medium">{region.metrics.activeOrders}</span>
                      </div>
                      <div className="flex justify-between text-xs">
                        <span className="text-muted-foreground">Avg Delivery:</span>
                        <span className="font-medium">{region.metrics.averageDeliveryTime}</span>
                      </div>
                      <div className="flex justify-between text-xs">
                        <span className="text-muted-foreground">Cost/Delivery:</span>
                        <span className="font-medium">${region.metrics.costPerDelivery}</span>
                      </div>
                      {region.metrics.pendingPickups > 0 && (
                        <div className="flex justify-between text-xs">
                          <span className="text-muted-foreground">Pending Pickups:</span>
                          <Badge variant="destructive" className="text-xs">
                            {region.metrics.pendingPickups}
                          </Badge>
                        </div>
                      )}
                    </div>

                    <div className="pt-2 border-t">
                      <div className="flex items-center gap-1 text-xs text-muted-foreground">
                        <User className="h-3 w-3" />
                        {region.operationalInfo.coordinator}
                      </div>
                      <div className="flex items-center gap-1 text-xs text-muted-foreground">
                        <Phone className="h-3 w-3" />
                        {region.operationalInfo.phone}
                      </div>
                    </div>

                    {region.characteristics.ferryDependent && (
                      <div className="flex items-center gap-1 text-xs text-blue-600 bg-blue-100 p-1 rounded">
                        <Navigation className="h-3 w-3" />
                        Ferry Dependent
                      </div>
                    )}
                    
                    {region.characteristics.requiresPooling && (
                      <div className="flex items-center gap-1 text-xs text-orange-600 bg-orange-100 p-1 rounded">
                        <Layers className="h-3 w-3" />
                        Pool Delivery
                      </div>
                    )}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>

          {/* Regional Order Distribution */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Package className="h-5 w-5" />
                  Orders by Region
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {regionalData.map(region => {
                    const regionOrders = availableOrders.filter(order => order.deliveryRegion === region.type);
                    return (
                      <div key={region.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                        <div className="flex items-center gap-3">
                          <div className={`w-3 h-3 rounded-full ${
                            region.type === 'local' ? 'bg-green-500' :
                            region.type === 'out-of-town' ? 'bg-blue-500' :
                            region.type === 'island-pool' ? 'bg-purple-500' :
                            region.type === 'okanagan-pool' ? 'bg-orange-500' :
                            'bg-gray-500'
                          }`} />
                          <div>
                            <p className="font-medium text-sm">{region.name}</p>
                            <p className="text-xs text-muted-foreground">
                              {regionOrders.reduce((sum, order) => sum + order.weight, 0).toLocaleString()} lbs
                            </p>
                          </div>
                        </div>
                        <div className="text-right">
                          <Badge variant="outline">{regionOrders.length} orders</Badge>
                          <p className="text-xs text-muted-foreground mt-1">
                            ${regionOrders.reduce((sum, order) => sum + order.value, 0).toLocaleString()}
                          </p>
                        </div>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Truck className="h-5 w-5" />
                  Fleet Assignment
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  {regionalData.map(region => {
                    const regionTrucks = availableTrucks.filter(truck => truck.assignedRegion === region.type);
                    const activeTrucks = regionTrucks.filter(truck => truck.status !== 'maintenance' && truck.status !== 'out-of-service');
                    return (
                      <div key={region.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                        <div className="flex items-center gap-3">
                          <div className={`w-3 h-3 rounded-full ${
                            region.type === 'local' ? 'bg-green-500' :
                            region.type === 'out-of-town' ? 'bg-blue-500' :
                            region.type === 'island-pool' ? 'bg-purple-500' :
                            region.type === 'okanagan-pool' ? 'bg-orange-500' :
                            'bg-gray-500'
                          }`} />
                          <div>
                            <p className="font-medium text-sm">{region.name}</p>
                            <p className="text-xs text-muted-foreground">
                              {activeTrucks.length} active • {regionTrucks.filter(t => t.poolTruck).length} pool trucks
                            </p>
                          </div>
                        </div>
                        <div className="text-right">
                          <Badge variant="outline">{regionTrucks.length} trucks</Badge>
                          {region.characteristics.ferryDependent && (
                            <p className="text-xs text-blue-600 mt-1">
                              {regionTrucks.filter(t => t.ferryCapable).length} ferry capable
                            </p>
                          )}
                        </div>
                      </div>
                    );
                  })}
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Regional Alerts & Notifications */}
          <div className="grid grid-cols-1 md:grid-cols-3 gap-4">
            <Card className="border-yellow-200 bg-yellow-50">
              <CardContent className="pt-4">
                <div className="flex items-center gap-3">
                  <AlertCircle className="h-5 w-5 text-yellow-600" />
                  <div>
                    <p className="font-medium text-yellow-800">Island Pool Alert</p>
                    <p className="text-sm text-yellow-700">
                      Ferry booking required for 8 orders by 4 PM today
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="border-blue-200 bg-blue-50">
              <CardContent className="pt-4">
                <div className="flex items-center gap-3">
                  <Info className="h-5 w-5 text-blue-600" />
                  <div>
                    <p className="font-medium text-blue-800">Okanagan Pool Ready</p>
                    <p className="text-sm text-blue-700">
                      Pool consolidation complete - ready for dispatch
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card className="border-green-200 bg-green-50">
              <CardContent className="pt-4">
                <div className="flex items-center gap-3">
                  <CheckCircle2 className="h-5 w-5 text-green-600" />
                  <div>
                    <p className="font-medium text-green-800">Customer Pickup</p>
                    <p className="text-sm text-green-700">
                      9 orders scheduled for pickup today
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="loads" className="space-y-4">
          {/* Summary Cards */}
          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <Truck className="h-4 w-4 text-blue-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Active Loads</p>
                    <p className="text-2xl font-medium">
                      {sampleLoads.filter(load => ['planning', 'loading', 'loaded', 'dispatched'].includes(load.status)).length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-orange-100 rounded-full">
                    <Navigation className="h-4 w-4 text-orange-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">In Transit</p>
                    <p className="text-2xl font-medium">
                      {sampleLoads.filter(load => load.status === 'in-transit').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-green-100 rounded-full">
                    <CheckCircle2 className="h-4 w-4 text-green-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Delivered Today</p>
                    <p className="text-2xl font-medium">
                      {sampleLoads.filter(load => 
                        load.status === 'delivered' && 
                        load.actualDelivery?.startsWith(new Date().toISOString().split('T')[0])
                      ).length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-red-100 rounded-full">
                    <AlertCircle className="h-4 w-4 text-red-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Overdue</p>
                    <p className="text-2xl font-medium">0</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Loads List */}
          <div className="space-y-4">
            {filteredLoads.map(load => (
              <Card key={load.id} className="hover:shadow-md transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <Truck className="h-5 w-5" />
                        {load.id}
                        <Badge variant="outline" className="capitalize">
                          {load.trailerType.replace('-', ' ')}
                        </Badge>
                      </CardTitle>
                      <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                        <span className="flex items-center gap-1">
                          <User className="h-3 w-3" />
                          {load.driverName}
                        </span>
                        <span>•</span>
                        <span>{load.truckNumber}</span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <Phone className="h-3 w-3" />
                          {load.driverPhone}
                        </span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className={getPriorityColor(load.priority)}>
                        {load.priority}
                      </Badge>
                      <Badge variant="outline" className={getStatusColor(load.status)}>
                        {load.status.replace('-', ' ')}
                      </Badge>
                      {load.deliveryRegion && (
                        <Badge variant="outline" className={
                          load.deliveryRegion === 'local' ? 'bg-green-100 text-green-700 border-green-200' :
                          load.deliveryRegion === 'out-of-town' ? 'bg-blue-100 text-blue-700 border-blue-200' :
                          load.deliveryRegion === 'island-pool' ? 'bg-purple-100 text-purple-700 border-purple-200' :
                          load.deliveryRegion === 'okanagan-pool' ? 'bg-orange-100 text-orange-700 border-orange-200' :
                          'bg-gray-100 text-gray-700 border-gray-200'
                        }>
                          {load.deliveryRegion.replace('-', ' ')}
                        </Badge>
                      )}
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {/* Load Utilization */}
                    <div className="space-y-2">
                      <div className="flex justify-between text-sm">
                        <span>Load Utilization</span>
                        <span className="flex items-center gap-2">
                          <Weight className="h-4 w-4" />
                          {load.currentWeight.toLocaleString()} / {load.maxWeight.toLocaleString()} lbs
                        </span>
                      </div>
                      <Progress value={load.utilizationPercentage} className="h-2" />
                      <div className="flex justify-between text-xs text-muted-foreground">
                        <span>{load.utilizationPercentage}% utilized</span>
                        <span>{load.items.length} stops</span>
                      </div>
                    </div>

                    {/* Schedule */}
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">Scheduled Pickup:</span>
                        <p className="font-medium flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          {new Date(load.scheduledPickup).toLocaleString()}
                        </p>
                      </div>
                      {load.estimatedDelivery && (
                        <div>
                          <span className="text-muted-foreground">Est. Delivery:</span>
                          <p className="font-medium flex items-center gap-1">
                            <Clock className="h-3 w-3" />
                            {new Date(load.estimatedDelivery).toLocaleString()}
                          </p>
                        </div>
                      )}
                    </div>

                    {/* Route & Stops */}
                    <div className="space-y-2">
                      <span className="text-muted-foreground text-sm">Route:</span>
                      <div className="space-y-2">
                        {load.items.map((item, index) => (
                          <div key={item.id} className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg">
                            <div className="flex-shrink-0 w-6 h-6 bg-teal-100 text-teal-700 rounded-full flex items-center justify-center text-xs font-medium">
                              {index + 1}
                            </div>
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center justify-between">
                                <div>
                                  <p className="font-medium text-sm">{item.customerName}</p>
                                  <p className="text-xs text-muted-foreground">{item.orderNumber}</p>
                                </div>
                                <div className="text-right text-xs">
                                  <p className="font-medium">{item.weight.toLocaleString()} lbs</p>
                                  <p className="text-muted-foreground">{item.pieces} pcs</p>
                                </div>
                              </div>
                              <div className="mt-1">
                                <p className="text-xs text-muted-foreground flex items-center gap-1">
                                  <MapPin className="h-3 w-3" />
                                  {item.destination}
                                </p>
                                {item.specialInstructions && (
                                  <p className="text-xs text-orange-600 mt-1 bg-orange-50 p-1 rounded">
                                    ⚠️ {item.specialInstructions}
                                  </p>
                                )}
                              </div>
                            </div>
                          </div>
                        ))}
                      </div>
                    </div>

                    {/* Notes */}
                    {load.notes && (
                      <div className="p-3 bg-blue-50 rounded-lg border border-blue-200">
                        <div className="flex items-start gap-2">
                          <FileText className="h-4 w-4 text-blue-600 mt-0.5 flex-shrink-0" />
                          <div>
                            <p className="text-sm font-medium text-blue-800">Load Notes</p>
                            <p className="text-sm text-blue-700">{load.notes}</p>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Actions */}
                    <div className="flex justify-between items-center pt-3 border-t">
                      <div className="text-xs text-muted-foreground">
                        Created by {load.createdBy} • {new Date(load.createdAt).toLocaleDateString()}
                      </div>
                      <div className="flex gap-2">
                        <Button variant="outline" size="sm">
                          <Edit className="h-4 w-4 mr-2" />
                          Edit Load
                        </Button>
                        {load.status === 'planning' && (
                          <Button size="sm">
                            <CheckCircle2 className="h-4 w-4 mr-2" />
                            Start Loading
                          </Button>
                        )}
                        {load.status === 'loaded' && (
                          <Button size="sm">
                            <Navigation className="h-4 w-4 mr-2" />
                            Dispatch
                          </Button>
                        )}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="planning" className="space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-xl font-semibold">Load Planning & Optimization</h2>
              <p className="text-muted-foreground">Build optimized loads from available orders</p>
            </div>
            <Button onClick={() => setShowLoadBuilder(true)}>
              <Plus className="h-4 w-4 mr-2" />
              New Load
            </Button>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Available Orders */}
            <div className="lg:col-span-2 space-y-4">
              <Card>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2">
                      <Package className="h-5 w-5" />
                      Available Orders ({availableOrders.length})
                    </CardTitle>
                    <div className="flex gap-2">
                      <Select value={loadPlannerFilters.priority} onValueChange={(value) => 
                        setLoadPlannerFilters({...loadPlannerFilters, priority: value})}>
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="all">All Priority</SelectItem>
                          <SelectItem value="urgent">Urgent</SelectItem>
                          <SelectItem value="high">High</SelectItem>
                          <SelectItem value="normal">Normal</SelectItem>
                          <SelectItem value="low">Low</SelectItem>
                        </SelectContent>
                      </Select>
                      <Select value={loadPlannerFilters.status} onValueChange={(value) => 
                        setLoadPlannerFilters({...loadPlannerFilters, status: value})}>
                        <SelectTrigger className="w-32">
                          <SelectValue />
                        </SelectTrigger>
                        <SelectContent>
                          <SelectItem value="all">All Status</SelectItem>
                          <SelectItem value="ready">Ready</SelectItem>
                          <SelectItem value="packed">Packed</SelectItem>
                          <SelectItem value="picked">Picked</SelectItem>
                        </SelectContent>
                      </Select>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3 max-h-96 overflow-y-auto">
                    {availableOrders
                      .filter(order => 
                        (loadPlannerFilters.priority === "all" || order.priority === loadPlannerFilters.priority) &&
                        (loadPlannerFilters.status === "all" || order.pickingStatus === loadPlannerFilters.status)
                      )
                      .map(order => (
                      <div key={order.id} className={`p-4 border rounded-lg cursor-pointer transition-all ${
                        selectedOrders.includes(order.id) 
                          ? 'border-teal-300 bg-teal-50' 
                          : 'border-gray-200 hover:border-gray-300'
                      }`} 
                      onClick={() => {
                        if (selectedOrders.includes(order.id)) {
                          setSelectedOrders(selectedOrders.filter(id => id !== order.id));
                        } else {
                          setSelectedOrders([...selectedOrders, order.id]);
                        }
                      }}>
                        <div className="flex items-center justify-between">
                          <div className="flex items-center gap-3">
                            <Checkbox 
                              checked={selectedOrders.includes(order.id)}
                              readOnly
                            />
                            <div>
                              <p className="font-medium">{order.customerName}</p>
                              <p className="text-sm text-muted-foreground">{order.orderNumber}</p>
                            </div>
                          </div>
                          <div className="flex items-center gap-2">
                            <Badge variant="outline" className={getPriorityColor(order.priority)}>
                              {order.priority}
                            </Badge>
                            <Badge variant="outline">{order.pickingStatus}</Badge>
                          </div>
                        </div>
                        <div className="mt-2 grid grid-cols-2 gap-4 text-sm">
                          <div>
                            <span className="text-muted-foreground">Destination:</span>
                            <p className="font-medium">{order.city}, {order.state}</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Distance:</span>
                            <p className="font-medium">{order.distance} mi</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Weight:</span>
                            <p className="font-medium">{order.weight.toLocaleString()} lbs</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Value:</span>
                            <p className="font-medium">${order.value.toLocaleString()}</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Due Date:</span>
                            <p className="font-medium">{new Date(order.requiredDeliveryDate).toLocaleDateString()}</p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">Pieces:</span>
                            <p className="font-medium">{order.pieces}</p>
                          </div>
                        </div>
                        {order.specialInstructions && (
                          <div className="mt-2 p-2 bg-yellow-50 rounded text-sm">
                            <span className="text-yellow-800">⚠️ {order.specialInstructions}</span>
                          </div>
                        )}
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </div>

            {/* Load Optimizer */}
            <div className="space-y-4">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Calculator className="h-5 w-5" />
                    Load Optimizer
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="p-3 bg-teal-50 rounded-lg">
                      <p className="text-sm font-medium text-teal-800">Selected Orders</p>
                      <p className="text-lg font-bold text-teal-900">{selectedOrders.length}</p>
                    </div>

                    {selectedOrders.length > 0 && (
                      <>
                        <div className="space-y-2">
                          <div className="flex justify-between text-sm">
                            <span>Total Weight:</span>
                            <span className="font-medium">
                              {availableOrders
                                .filter(order => selectedOrders.includes(order.id))
                                .reduce((sum, order) => sum + order.weight, 0)
                                .toLocaleString()} lbs
                            </span>
                          </div>
                          <div className="flex justify-between text-sm">
                            <span>Total Value:</span>
                            <span className="font-medium">
                              ${availableOrders
                                .filter(order => selectedOrders.includes(order.id))
                                .reduce((sum, order) => sum + order.value, 0)
                                .toLocaleString()}
                            </span>
                          </div>
                          <div className="flex justify-between text-sm">
                            <span>Total Distance:</span>
                            <span className="font-medium">
                              {availableOrders
                                .filter(order => selectedOrders.includes(order.id))
                                .reduce((sum, order) => sum + order.distance, 0)} mi
                            </span>
                          </div>
                        </div>

                        <div className="space-y-2">
                          <p className="text-sm font-medium">Recommended Truck:</p>
                          {availableTrucks
                            .filter(truck => truck.status === "available")
                            .map(truck => {
                              const totalWeight = availableOrders
                                .filter(order => selectedOrders.includes(order.id))
                                .reduce((sum, order) => sum + order.weight, 0);
                              const utilization = Math.round((totalWeight / truck.maxWeight) * 100);
                              
                              return (
                                <div key={truck.id} className="p-3 border rounded-lg">
                                  <div className="flex justify-between items-center">
                                    <div>
                                      <p className="font-medium">{truck.number}</p>
                                      <p className="text-sm text-muted-foreground capitalize">
                                        {truck.type.replace('-', ' ')}
                                      </p>
                                    </div>
                                    <Badge variant={utilization > 90 ? "destructive" : utilization > 70 ? "default" : "secondary"}>
                                      {utilization}%
                                    </Badge>
                                  </div>
                                  <Progress value={utilization} className="mt-2" />
                                </div>
                              );
                            })}
                        </div>

                        <div className="space-y-2">
                          <Button 
                            className="w-full" 
                            disabled={selectedOrders.length === 0}
                            onClick={() => setShowLoadBuilder(true)}
                          >
                            <Truck className="h-4 w-4 mr-2" />
                            Create Full Load
                          </Button>
                          <Button 
                            variant="outline"
                            className="w-full" 
                            disabled={selectedOrders.length === 0}
                            onClick={() => setShowLoadBuilder(true)}
                          >
                            <Package className="h-4 w-4 mr-2" />
                            Create Partial Load
                          </Button>
                        </div>
                      </>
                    )}
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Route className="h-5 w-5" />
                    Route Optimization
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    <div className="p-3 border rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <Target className="h-4 w-4 text-green-600" />
                        <span className="text-sm font-medium">Optimized Route</span>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        AI-powered route optimization considers traffic, delivery windows, and fuel efficiency
                      </p>
                    </div>
                    <div className="p-3 border rounded-lg">
                      <div className="flex items-center gap-2 mb-2">
                        <Fuel className="h-4 w-4 text-blue-600" />
                        <span className="text-sm font-medium">Fuel Efficiency</span>
                      </div>
                      <p className="text-xs text-muted-foreground">
                        Estimated savings: $25-40 per route with optimal planning
                      </p>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="tracking" className="space-y-4">
          <div className="space-y-4">
            {sampleLoads
              .filter(load => ['dispatched', 'in-transit', 'delivered'].includes(load.status))
              .map(load => (
              <Card key={load.id}>
                <CardHeader>
                  <div className="flex items-center justify-between">
                    <CardTitle className="flex items-center gap-2">
                      <Navigation className="h-5 w-5" />
                      {load.id} - {load.truckNumber}
                    </CardTitle>
                    <Badge variant="outline" className={getStatusColor(load.status)}>
                      {load.status.replace('-', ' ')}
                    </Badge>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">Driver:</span>
                        <p className="font-medium">{load.driverName}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Phone:</span>
                        <p className="font-medium">{load.driverPhone}</p>
                      </div>
                    </div>

                    {load.actualPickup && (
                      <div className="text-sm">
                        <span className="text-muted-foreground">Actual Pickup:</span>
                        <p className="font-medium">{new Date(load.actualPickup).toLocaleString()}</p>
                      </div>
                    )}

                    {load.actualDelivery && (
                      <div className="text-sm">
                        <span className="text-muted-foreground">Delivered:</span>
                        <p className="font-medium text-green-600">{new Date(load.actualDelivery).toLocaleString()}</p>
                      </div>
                    )}

                    <div className="space-y-2">
                      <span className="text-muted-foreground text-sm">Delivery Progress:</span>
                      <div className="space-y-2">
                        {load.items.map((item, index) => (
                          <div key={item.id} className="flex items-center gap-3">
                            <div className={`w-4 h-4 rounded-full flex-shrink-0 ${
                              load.status === 'delivered' ? 'bg-green-500' : 
                              load.status === 'in-transit' ? 'bg-orange-500' : 'bg-gray-300'
                            }`} />
                            <div className="flex-1">
                              <p className="text-sm font-medium">{item.customerName}</p>
                              <p className="text-xs text-muted-foreground">{item.destination}</p>
                            </div>
                            {load.status === 'delivered' && (
                              <CheckCircle2 className="h-4 w-4 text-green-500" />
                            )}
                          </div>
                        ))}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="pools" className="space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-xl font-semibold">Pool Truck Management</h2>
              <p className="text-muted-foreground">Manage consolidated deliveries for Island, Okanagan, and Out-of-Town routes</p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm">
                <Timer className="h-4 w-4 mr-2" />
                Schedule Pool
              </Button>
              <Button size="sm" className="bg-teal-600 hover:bg-teal-700">
                <Plus className="h-4 w-4 mr-2" />
                Create Pool Load
              </Button>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-purple-100 rounded-full">
                    <Navigation className="h-4 w-4 text-purple-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Island Pool</p>
                    <p className="text-2xl font-medium">8</p>
                    <p className="text-xs text-muted-foreground">orders waiting</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-orange-100 rounded-full">
                    <MapPin className="h-4 w-4 text-orange-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Okanagan Pool</p>
                    <p className="text-2xl font-medium">5</p>
                    <p className="text-xs text-muted-foreground">orders waiting</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <Route className="h-4 w-4 text-blue-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Out-of-Town</p>
                    <p className="text-2xl font-medium">3</p>
                    <p className="text-xs text-muted-foreground">orders waiting</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-green-100 rounded-full">
                    <DollarSign className="h-4 w-4 text-green-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Pool Savings</p>
                    <p className="text-2xl font-medium">$1,240</p>
                    <p className="text-xs text-muted-foreground">this week</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Island Pool Management */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Navigation className="h-5 w-5 text-purple-600" />
                  Island Pool Trucks
                </CardTitle>
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Badge variant="outline" className="text-purple-600 border-purple-200">Ferry Dependent</Badge>
                  <span>Next departure: 2:00 PM</span>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-purple-50 border-purple-200">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">VI-SOUTH-01</p>
                        <p className="text-sm text-muted-foreground">T-312 • Victoria Route</p>
                      </div>
                      <Badge variant="outline" className="bg-purple-100 text-purple-700">
                        Ready to Ship
                      </Badge>
                    </div>
                    
                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">78% (36,400 lbs)</span>
                      </div>
                      <Progress value={78} className="h-2" />
                    </div>

                    <div className="space-y-2 mb-3">
                      <p className="text-sm font-medium">Orders (2):</p>
                      <div className="space-y-1">
                        <div className="flex justify-between text-xs">
                          <span>VSW-5567 • Victoria Steel Works</span>
                          <span>6,200 lbs</span>
                        </div>
                        <div className="flex justify-between text-xs">
                          <span>NB-9987 • Nanaimo Builders</span>
                          <span>5,200 lbs</span>
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-2 text-xs text-purple-600 mb-3">
                      <Navigation className="h-3 w-3" />
                      Ferry: Tsawwassen-Swartz Bay • $350 total
                    </div>

                    <div className="flex gap-2">
                      <Button size="sm" variant="outline" className="flex-1">
                        <Plus className="h-4 w-4 mr-1" />
                        Add Order
                      </Button>
                      <Button size="sm" className="bg-purple-600 hover:bg-purple-700">
                        <CheckCircle2 className="h-4 w-4 mr-1" />
                        Dispatch
                      </Button>
                    </div>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">VI-CENTRAL-01</p>
                        <p className="text-sm text-muted-foreground">T-789 • Nanaimo Route</p>
                      </div>
                      <Badge variant="outline" className="bg-yellow-100 text-yellow-700">
                        Consolidating
                      </Badge>
                    </div>
                    
                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">35% (16,100 lbs)</span>
                      </div>
                      <Progress value={35} className="h-2" />
                    </div>

                    <div className="text-xs text-muted-foreground mb-3">
                      Consolidation deadline: Tomorrow 10:00 AM
                    </div>

                    <div className="flex gap-2">
                      <Button size="sm" variant="outline" className="flex-1">
                        <Timer className="h-4 w-4 mr-1" />
                        Extend Deadline
                      </Button>
                      <Button size="sm" variant="outline">
                        <Edit className="h-4 w-4 mr-1" />
                        Manage
                      </Button>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Okanagan Pool Management */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <MapPin className="h-5 w-5 text-orange-600" />
                  Okanagan Pool Trucks
                </CardTitle>
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Badge variant="outline" className="text-orange-600 border-orange-200">Mountain Routes</Badge>
                  <span>Weekly service</span>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-orange-50 border-orange-200">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">OK-CENTRAL-01</p>
                        <p className="text-sm text-muted-foreground">T-098 • Kelowna Route</p>
                      </div>
                      <Badge variant="outline" className="bg-orange-100 text-orange-700">
                        Consolidating
                      </Badge>
                    </div>
                    
                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">62% (24,800 lbs)</span>
                      </div>
                      <Progress value={62} className="h-2" />
                    </div>

                    <div className="space-y-2 mb-3">
                      <p className="text-sm font-medium">Orders (1):</p>
                      <div className="text-xs">KCS-8854 • Kelowna Construction Supply • 8,900 lbs</div>
                    </div>

                    <div className="text-xs text-muted-foreground mb-3">
                      Next departure: Friday 6:00 AM
                    </div>

                    <div className="flex gap-2">
                      <Button size="sm" variant="outline" className="flex-1">
                        <Plus className="h-4 w-4 mr-1" />
                        Add Order
                      </Button>
                      <Button size="sm" variant="outline">
                        <Timer className="h-4 w-4 mr-1" />
                        Schedule
                      </Button>
                    </div>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">OK-NORTH-01</p>
                        <p className="text-sm text-muted-foreground">Available • Vernon/Kamloops</p>
                      </div>
                      <Badge variant="outline" className="bg-gray-100 text-gray-700">
                        Available
                      </Badge>
                    </div>
                    
                    <div className="text-xs text-muted-foreground mb-3">
                      No orders assigned
                    </div>

                    <Button size="sm" variant="outline" className="w-full">
                      <Plus className="h-4 w-4 mr-1" />
                      Create Pool Load
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Out-of-Town Pool Management */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Route className="h-5 w-5 text-blue-600" />
                  Out-of-Town Lanes
                </CardTitle>
                <div className="flex items-center gap-2 text-sm text-muted-foreground">
                  <Badge variant="outline" className="text-blue-600 border-blue-200">Multi-Lane</Badge>
                  <span>2-3x weekly</span>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-blue-50 border-blue-200">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">AB-NORTH-01</p>
                        <p className="text-sm text-muted-foreground">T-401 • Calgary/Edmonton</p>
                      </div>
                      <Badge variant="outline" className="bg-blue-100 text-blue-700">
                        In Transit
                      </Badge>
                    </div>
                    
                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">85% (40,800 lbs)</span>
                      </div>
                      <Progress value={85} className="h-2" />
                    </div>

                    <div className="space-y-2 mb-3">
                      <p className="text-sm font-medium">Orders (1):</p>
                      <div className="text-xs">CSD-7789 • Calgary Steel Dynamics • 12,400 lbs</div>
                    </div>

                    <div className="flex items-center gap-2 text-xs text-blue-600 mb-3">
                      <Truck className="h-3 w-3" />
                      ETA Calgary: Tomorrow 2:00 PM
                    </div>

                    <Button size="sm" variant="outline" className="w-full">
                      <Navigation className="h-4 w-4 mr-1" />
                      Track Load
                    </Button>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">AB-SOUTH-01</p>
                        <p className="text-sm text-muted-foreground">T-432 • Medicine Hat/Lethbridge</p>
                      </div>
                      <Badge variant="outline" className="bg-gray-100 text-gray-700">
                        Available
                      </Badge>
                    </div>
                    
                    <div className="text-xs text-muted-foreground mb-3">
                      Ready for next consolidation
                    </div>

                    <Button size="sm" variant="outline" className="w-full">
                      <Plus className="h-4 w-4 mr-1" />
                      Build Pool Load
                    </Button>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Pool Optimization Recommendations */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Target className="h-5 w-5" />
                  Pool Optimization Recommendations
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-green-50 border-green-200">
                    <div className="flex items-start gap-3">
                      <div className="p-1 bg-green-100 rounded-full">
                        <CheckCircle2 className="h-4 w-4 text-green-600" />
                      </div>
                      <div className="flex-1">
                        <p className="font-medium text-green-800">Island Pool Ready</p>
                        <p className="text-sm text-green-700 mb-2">
                          VI-SOUTH-01 has optimal load and ferry booking confirmed
                        </p>
                        <div className="space-y-1 text-xs text-green-600">
                          <p>• 78% utilization achieved</p>
                          <p>• Ferry booking confirmed for 2:00 PM</p>
                          <p>• Saves $320 vs individual deliveries</p>
                        </div>
                        <Button size="sm" className="mt-3 bg-green-600 hover:bg-green-700">
                          Dispatch Pool
                        </Button>
                      </div>
                    </div>
                  </div>

                  <div className="p-4 border rounded-lg bg-yellow-50 border-yellow-200">
                    <div className="flex items-start gap-3">
                      <div className="p-1 bg-yellow-100 rounded-full">
                        <AlertCircle className="h-4 w-4 text-yellow-600" />
                      </div>
                      <div className="flex-1">
                        <p className="font-medium text-yellow-800">Okanagan Consolidation</p>
                        <p className="text-sm text-yellow-700 mb-2">
                          Wait for 2 more orders to optimize OK-CENTRAL-01
                        </p>
                        <div className="space-y-1 text-xs text-yellow-600">
                          <p>• Current: 62% utilization</p>
                          <p>• Target: 85% with 2 more orders</p>
                          <p>• Deadline: Friday morning</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Users className="h-5 w-5" />
                  Customer Pickup Coordination
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-blue-50 border-blue-200">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">Today's Pickups</p>
                        <p className="text-sm text-muted-foreground">9 orders scheduled</p>
                      </div>
                      <Badge variant="outline" className="bg-blue-100 text-blue-700">
                        Active
                      </Badge>
                    </div>

                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>PFW-4412 • Pacific Fabrication</span>
                        <span>10:00 AM</span>
                      </div>
                      <div className="flex justify-between text-sm">
                        <span>2 more scheduled</span>
                        <span>This afternoon</span>
                      </div>
                    </div>

                    <Button size="sm" variant="outline" className="w-full">
                      <Calendar className="h-4 w-4 mr-1" />
                      Manage Schedule
                    </Button>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <p className="font-medium mb-2">Pickup Metrics</p>
                    <div className="space-y-2 text-sm">
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Orders ready:</span>
                        <span>15</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Avg wait time:</span>
                        <span>12 minutes</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Cost savings:</span>
                        <span className="text-green-600">$0/delivery</span>
                      </div>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="execution" className="space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-xl font-semibold">Shipping Execution</h2>
              <p className="text-muted-foreground">Manage active shipments and dispatch operations</p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {/* Status Summary Cards */}
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-yellow-100 rounded-full">
                    <Clock className="h-4 w-4 text-yellow-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Ready to Load</p>
                    <p className="text-2xl font-medium">
                      {sampleLoads.filter(load => load.status === 'planning').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <Package className="h-4 w-4 text-blue-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Loading</p>
                    <p className="text-2xl font-medium">
                      {sampleLoads.filter(load => load.status === 'loading').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-purple-100 rounded-full">
                    <Truck className="h-4 w-4 text-purple-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Ready to Dispatch</p>
                    <p className="text-2xl font-medium">
                      {sampleLoads.filter(load => load.status === 'loaded').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-orange-100 rounded-full">
                    <Navigation className="h-4 w-4 text-orange-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">In Transit</p>
                    <p className="text-2xl font-medium">
                      {sampleLoads.filter(load => load.status === 'in-transit').length}
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Partial Loads Alert */}
          <Card className="border-blue-200 bg-blue-50">
            <CardContent className="pt-6">
              <div className="flex items-center gap-3">
                <div className="p-2 bg-blue-100 rounded-full">
                  <Info className="h-4 w-4 text-blue-600" />
                </div>
                <div className="flex-1">
                  <p className="font-medium text-blue-800">Partial Load Optimization Available</p>
                  <p className="text-sm text-blue-600">
                    3 partial loads can be consolidated for better efficiency. 
                    <Button variant="link" className="h-auto p-0 text-blue-600">
                      View recommendations →
                    </Button>
                  </p>
                </div>
                <Button variant="outline" size="sm" className="text-blue-600 border-blue-300">
                  Optimize Loads
                </Button>
              </div>
            </CardContent>
          </Card>

          {/* Execution Dashboard */}
          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Loads Ready for Dispatch */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <Play className="h-5 w-5" />
                  Ready for Dispatch
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {sampleLoads
                    .filter(load => ['planning', 'loading', 'loaded'].includes(load.status))
                    .map(load => (
                    <div key={load.id} className="p-4 border rounded-lg">
                      <div className="flex items-center justify-between mb-3">
                        <div>
                          <p className="font-medium">{load.id}</p>
                          <p className="text-sm text-muted-foreground">
                            {load.driverName} • {load.truckNumber}
                          </p>
                        </div>
                        <div className="flex items-center gap-2">
                          <Badge variant="outline" className={getStatusColor(load.status)}>
                            {load.status.replace('-', ' ')}
                          </Badge>
                          <Badge variant="outline" className={getPriorityColor(load.priority)}>
                            {load.priority}
                          </Badge>
                        </div>
                      </div>

                      <div className="grid grid-cols-2 gap-4 text-sm mb-3">
                        <div>
                          <span className="text-muted-foreground">Scheduled Pickup:</span>
                          <p className="font-medium">
                            {new Date(load.scheduledPickup).toLocaleDateString()}
                          </p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Stops:</span>
                          <p className="font-medium">{load.items.length}</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Weight:</span>
                          <p className="font-medium">{load.currentWeight.toLocaleString()} lbs</p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">Utilization:</span>
                          <p className="font-medium">{load.utilizationPercentage}%</p>
                        </div>
                      </div>

                      <div className="flex justify-end gap-2">
                        {load.status === 'planning' && (
                          <Button size="sm" variant="outline">
                            <Package className="h-4 w-4 mr-2" />
                            Start Loading
                          </Button>
                        )}
                        {load.status === 'loading' && (
                          <Button size="sm" variant="outline">
                            <CheckCircle2 className="h-4 w-4 mr-2" />
                            Mark Loaded
                          </Button>
                        )}
                        {load.status === 'loaded' && (
                          <Button 
                            size="sm" 
                            onClick={() => {
                              setSelectedLoad(load);
                              setShowDispatchDialog(true);
                            }}
                            className="bg-teal-600 hover:bg-teal-700"
                          >
                            <Play className="h-4 w-4 mr-2" />
                            Dispatch
                          </Button>
                        )}
                        <Button size="sm" variant="outline">
                          <Edit className="h-4 w-4 mr-2" />
                          Edit
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>

            {/* Driver Communication */}
            <Card>
              <CardHeader>
                <CardTitle className="flex items-center gap-2">
                  <MessageSquare className="h-5 w-5" />
                  Driver Communication
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {sampleLoads
                    .filter(load => ['dispatched', 'in-transit'].includes(load.status))
                    .map(load => (
                    <div key={load.id} className="p-4 border rounded-lg">
                      <div className="flex items-center justify-between mb-2">
                        <div>
                          <p className="font-medium">{load.driverName}</p>
                          <p className="text-sm text-muted-foreground">{load.id}</p>
                        </div>
                        <Badge variant="outline" className="bg-green-100 text-green-800">
                          Online
                        </Badge>
                      </div>
                      <div className="flex gap-2">
                        <Button size="sm" variant="outline">
                          <Phone className="h-4 w-4 mr-2" />
                          Call
                        </Button>
                        <Button size="sm" variant="outline">
                          <MessageSquare className="h-4 w-4 mr-2" />
                          Message
                        </Button>
                        <Button size="sm" variant="outline">
                          <Locate className="h-4 w-4 mr-2" />
                          Track
                        </Button>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        <TabsContent value="tracking" className="space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-xl font-semibold">Live Tracking</h2>
              <p className="text-muted-foreground">Real-time location and status tracking for active shipments</p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm">
                <Locate className="h-4 w-4 mr-2" />
                Refresh All
              </Button>
              <Button variant="outline" size="sm">
                <MessageSquare className="h-4 w-4 mr-2" />
                Broadcast Message
              </Button>
            </div>
          </div>

          {/* Live Tracking Dashboard */}
          <div className="grid grid-cols-1 lg:grid-cols-3 gap-6">
            {/* Active Shipments */}
            <div className="lg:col-span-2">
              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Navigation className="h-5 w-5" />
                    Active Shipments
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    {sampleLoads
                      .filter(load => ['dispatched', 'in-transit'].includes(load.status))
                      .map(load => (
                      <div key={load.id} className="p-4 border rounded-lg">
                        <div className="flex items-center justify-between mb-3">
                          <div>
                            <p className="font-medium">{load.id}</p>
                            <p className="text-sm text-muted-foreground">
                              {load.driverName} • {load.truckNumber}
                            </p>
                          </div>
                          <div className="flex items-center gap-2">
                            <Badge variant="outline" className="bg-green-100 text-green-700">
                              Live
                            </Badge>
                            <Badge variant="outline" className={getStatusColor(load.status)}>
                              {load.status.replace('-', ' ')}
                            </Badge>
                          </div>
                        </div>

                        {load.currentLocation && (
                          <div className="mb-3 p-3 bg-blue-50 rounded-lg">
                            <div className="flex items-start gap-2">
                              <MapPin className="h-4 w-4 text-blue-600 mt-0.5" />
                              <div className="flex-1">
                                <p className="text-sm font-medium text-blue-800">Current Location</p>
                                <p className="text-sm text-blue-700">{load.currentLocation.address}</p>
                                <p className="text-xs text-blue-600">
                                  Last updated: {new Date(load.currentLocation.timestamp).toLocaleString()}
                                </p>
                              </div>
                            </div>
                          </div>
                        )}

                        <div className="mb-3">
                          <p className="text-sm font-medium mb-2">Route Progress:</p>
                          <div className="space-y-2">
                            {load.items.map((item, index) => (
                              <div key={item.id} className="flex items-center gap-3">
                                <div className={`flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center text-xs font-medium ${
                                  load.status === 'in-transit' && index === 0 ? 'bg-orange-500 text-white' : 'bg-gray-300 text-gray-600'
                                }`}>
                                  {index + 1}
                                </div>
                                <div className="flex-1 min-w-0">
                                  <p className="font-medium text-sm">{item.customerName}</p>
                                  <p className="text-xs text-muted-foreground">{item.destination}</p>
                                </div>
                                {load.status === 'delivered' && (
                                  <CheckCircle2 className="h-4 w-4 text-green-500" />
                                )}
                              </div>
                            ))}
                          </div>
                        </div>

                        <div className="flex justify-between items-center">
                          <div className="text-xs text-muted-foreground">
                            Est. delivery: {load.estimatedDelivery ? new Date(load.estimatedDelivery).toLocaleString() : 'TBD'}
                          </div>
                          <div className="flex gap-2">
                            <Button size="sm" variant="outline">
                              <Phone className="h-4 w-4 mr-1" />
                              Call
                            </Button>
                            <Button size="sm" variant="outline">
                              <MessageSquare className="h-4 w-4 mr-1" />
                              Message
                            </Button>
                            <Button size="sm" variant="outline">
                              <Navigation className="h-4 w-4 mr-1" />
                              Map
                            </Button>
                          </div>
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            </div>

            {/* Tracking Summary */}
            <div>
              <Card className="mb-6">
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <Target className="h-5 w-5" />
                    Tracking Summary
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Active Shipments:</span>
                      <span className="font-medium">
                        {sampleLoads.filter(load => ['dispatched', 'in-transit'].includes(load.status)).length}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">On Schedule:</span>
                      <span className="font-medium text-green-600">
                        {sampleLoads.filter(load => load.status === 'in-transit').length}
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Delayed:</span>
                      <span className="font-medium text-red-600">0</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">Exceptions:</span>
                      <span className="font-medium text-orange-600">0</span>
                    </div>
                  </div>
                </CardContent>
              </Card>

              <Card>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <AlertCircle className="h-5 w-5" />
                    Alerts & Notifications
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    <div className="p-3 bg-green-50 border border-green-200 rounded-lg">
                      <div className="flex items-center gap-2">
                        <CheckCircle2 className="h-4 w-4 text-green-600" />
                        <div>
                          <p className="text-sm font-medium text-green-800">All Systems Normal</p>
                          <p className="text-xs text-green-600">All shipments tracking properly</p>
                        </div>
                      </div>
                    </div>
                    
                    <div className="p-3 bg-blue-50 border border-blue-200 rounded-lg">
                      <div className="flex items-center gap-2">
                        <Info className="h-4 w-4 text-blue-600" />
                        <div>
                          <p className="text-sm font-medium text-blue-800">Ferry Schedule Update</p>
                          <p className="text-xs text-blue-600">Island routes updated for weather</p>
                        </div>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            </div>
          </div>
        </TabsContent>

        <TabsContent value="analytics" className="space-y-4">
          <div className="flex justify-between items-center">
            <div>
              <h2 className="text-xl font-semibold">Shipping Analytics</h2>
              <p className="text-muted-foreground">Performance metrics and insights</p>
            </div>
          </div>

          <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
            {/* KPI Cards */}
            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-green-100 rounded-full">
                    <Target className="h-4 w-4 text-green-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">On-Time Delivery</p>
                    <p className="text-2xl font-medium">95.2%</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-blue-100 rounded-full">
                    <Truck className="h-4 w-4 text-blue-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Avg Load Utilization</p>
                    <p className="text-2xl font-medium">78.5%</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-purple-100 rounded-full">
                    <DollarSign className="h-4 w-4 text-purple-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Cost Per Mile</p>
                    <p className="text-2xl font-medium">$2.85</p>
                  </div>
                </div>
              </CardContent>
            </Card>

            <Card>
              <CardContent className="pt-6">
                <div className="flex items-center">
                  <div className="p-2 bg-orange-100 rounded-full">
                    <Fuel className="h-4 w-4 text-orange-600" />
                  </div>
                  <div className="ml-4">
                    <p className="text-sm text-muted-foreground">Fuel Efficiency</p>
                    <p className="text-2xl font-medium">6.8 MPG</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Performance Trends */}
            <Card>
              <CardHeader>
                <CardTitle>Delivery Performance Trends</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid grid-cols-3 gap-4 text-center">
                    <div className="p-3 bg-green-50 rounded-lg">
                      <p className="text-sm text-muted-foreground">This Week</p>
                      <p className="text-lg font-medium text-green-600">96.8%</p>
                    </div>
                    <div className="p-3 bg-blue-50 rounded-lg">
                      <p className="text-sm text-muted-foreground">This Month</p>
                      <p className="text-lg font-medium text-blue-600">94.2%</p>
                    </div>
                    <div className="p-3 bg-purple-50 rounded-lg">
                      <p className="text-sm text-muted-foreground">This Quarter</p>
                      <p className="text-lg font-medium text-purple-600">95.1%</p>
                    </div>
                  </div>
                </div>
              </CardContent>
            </Card>

            {/* Fleet Utilization */}
            <Card>
              <CardHeader>
                <CardTitle>Fleet Utilization</CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-3">
                  {availableTrucks.map(truck => (
                    <div key={truck.id} className="flex items-center justify-between">
                      <div>
                        <p className="font-medium">{truck.number}</p>
                        <p className="text-sm text-muted-foreground capitalize">
                          {truck.type.replace('-', ' ')}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-medium">
                          {truck.status === 'assigned' ? '85%' : truck.status === 'available' ? '0%' : 'N/A'}
                        </p>
                        <Badge variant={
                          truck.status === 'assigned' ? 'default' : 
                          truck.status === 'available' ? 'secondary' : 'destructive'
                        }>
                          {truck.status}
                        </Badge>
                      </div>
                    </div>
                  ))}
                </div>
              </CardContent>
            </Card>
          </div>
        </TabsContent>

        {/* Load Builder Dialog */}
        <Dialog open={showLoadBuilder} onOpenChange={setShowLoadBuilder}>
          <DialogContent className="max-w-5xl">
            <DialogHeader>
              <DialogTitle>Create New Load</DialogTitle>
              <DialogDescription>
                Configure truck, driver, and scheduling details for the new load. Select orders to include.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-6">
              {/* Load Type Selection */}
              <div className="p-4 border rounded-lg bg-gray-50">
                <div className="flex items-center gap-4">
                  <div className="flex items-center gap-2">
                    <input
                      type="radio"
                      id="fullLoad"
                      name="loadType"
                      checked={!newLoad.isPartialLoad}
                      onChange={() => setNewLoad({...newLoad, isPartialLoad: false, maxUtilization: 100})}
                      className="text-teal-600"
                    />
                    <label htmlFor="fullLoad" className="text-sm font-medium">Full Load</label>
                  </div>
                  <div className="flex items-center gap-2">
                    <input
                      type="radio"
                      id="partialLoad"
                      name="loadType"
                      checked={newLoad.isPartialLoad}
                      onChange={() => setNewLoad({...newLoad, isPartialLoad: true, maxUtilization: 70})}
                      className="text-teal-600"
                    />
                    <label htmlFor="partialLoad" className="text-sm font-medium">Partial Load</label>
                  </div>
                  {newLoad.isPartialLoad && (
                    <div className="ml-4 flex items-center gap-2">
                      <label className="text-sm">Max Utilization:</label>
                      <Input
                        type="number"
                        min="20"
                        max="100"
                        value={newLoad.maxUtilization}
                        onChange={(e) => setNewLoad({...newLoad, maxUtilization: parseInt(e.target.value)})}
                        className="w-20"
                      />
                      <span className="text-sm">%</span>
                    </div>
                  )}
                </div>
                {newLoad.isPartialLoad && (
                  <p className="text-sm text-muted-foreground mt-2">
                    Partial loads allow for consolidation with other orders and faster dispatch times.
                  </p>
                )}
              </div>

              <div className="grid grid-cols-2 gap-6">
                {/* Load Configuration */}
                <div className="space-y-4">
                  <h3 className="font-medium">Load Configuration</h3>
                  <div className="grid grid-cols-1 gap-4">
                    <div>
                      <label className="text-sm font-medium">Truck</label>
                      <Select value={newLoad.truckNumber} onValueChange={(value) => 
                        setNewLoad({...newLoad, truckNumber: value})}>
                        <SelectTrigger>
                          <SelectValue placeholder="Select truck..." />
                        </SelectTrigger>
                        <SelectContent>
                          {availableTrucks
                            .filter(truck => truck.status === "available")
                            .map(truck => (
                            <SelectItem key={truck.id} value={truck.number}>
                              {truck.number} - {truck.type.replace('-', ' ')} ({truck.maxWeight.toLocaleString()} lbs)
                            </SelectItem>
                          ))}
                        </SelectContent>
                      </Select>
                    </div>
                    <div>
                      <label className="text-sm font-medium">Driver</label>
                      <Input 
                        value={newLoad.driverName}
                        onChange={(e) => setNewLoad({...newLoad, driverName: e.target.value})}
                        placeholder="Driver name..."
                      />
                    </div>
                    <div>
                      <label className="text-sm font-medium">Driver Phone</label>
                      <Input 
                        value={newLoad.driverPhone}
                        onChange={(e) => setNewLoad({...newLoad, driverPhone: e.target.value})}
                        placeholder="(555) 123-4567"
                      />
                    </div>
                    <div>
                      <label className="text-sm font-medium">Scheduled Pickup</label>
                      <Input 
                        type="datetime-local"
                        value={newLoad.scheduledPickup}
                        onChange={(e) => setNewLoad({...newLoad, scheduledPickup: e.target.value})}
                      />
                    </div>
                  </div>

                  <div>
                    <label className="text-sm font-medium">Load Notes</label>
                    <Textarea 
                      value={newLoad.notes}
                      onChange={(e) => setNewLoad({...newLoad, notes: e.target.value})}
                      placeholder="Special instructions or notes..."
                      rows={3}
                    />
                  </div>
                </div>

                {/* Selected Orders */}
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <h3 className="font-medium">Selected Orders ({selectedOrders.length})</h3>
                    <Button variant="outline" size="sm" onClick={() => setSelectedOrders([])}>
                      Clear All
                    </Button>
                  </div>
                  
                  <div className="border rounded-lg p-4 bg-gray-50 max-h-64 overflow-y-auto">
                    {selectedOrders.length === 0 ? (
                      <p className="text-center text-muted-foreground py-8">
                        No orders selected. Go back to Load Planning to select orders.
                      </p>
                    ) : (
                      <div className="space-y-3">
                        {availableOrders
                          .filter(order => selectedOrders.includes(order.id))
                          .map(order => (
                          <div key={order.id} className="flex items-center justify-between p-3 bg-white rounded border">
                            <div>
                              <p className="font-medium text-sm">{order.customerName}</p>
                              <p className="text-xs text-muted-foreground">{order.orderNumber}</p>
                              <p className="text-xs text-muted-foreground">{order.city}, {order.state}</p>
                            </div>
                            <div className="text-right">
                              <p className="text-sm font-medium">{order.weight.toLocaleString()} lbs</p>
                              <p className="text-xs text-muted-foreground">{order.pieces} pcs</p>
                              <Button
                                size="sm"
                                variant="ghost"
                                onClick={() => setSelectedOrders(selectedOrders.filter(id => id !== order.id))}
                              >
                                <X className="h-3 w-3" />
                              </Button>
                            </div>
                          </div>
                        ))}
                      </div>
                    )}
                  </div>

                  {/* Load Summary */}
                  {selectedOrders.length > 0 && (
                    <div className="p-4 border rounded-lg">
                      <h4 className="font-medium mb-3">Load Summary</h4>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span>Total Weight:</span>
                          <span className="font-medium">
                            {availableOrders
                              .filter(order => selectedOrders.includes(order.id))
                              .reduce((sum, order) => sum + order.weight, 0)
                              .toLocaleString()} lbs
                          </span>
                        </div>
                        <div className="flex justify-between">
                          <span>Total Value:</span>
                          <span className="font-medium">
                            ${availableOrders
                              .filter(order => selectedOrders.includes(order.id))
                              .reduce((sum, order) => sum + order.value, 0)
                              .toLocaleString()}
                          </span>
                        </div>
                        <div className="flex justify-between">
                          <span>Stops:</span>
                          <span className="font-medium">{selectedOrders.length}</span>
                        </div>
                        {newLoad.truckNumber && (
                          <div className="flex justify-between">
                            <span>Utilization:</span>
                            <span className="font-medium">
                              {Math.round((availableOrders
                                .filter(order => selectedOrders.includes(order.id))
                                .reduce((sum, order) => sum + order.weight, 0) / 
                                (availableTrucks.find(t => t.number === newLoad.truckNumber)?.maxWeight || 1)) * 100)}%
                            </span>
                          </div>
                        )}
                      </div>
                      
                      {newLoad.isPartialLoad && (
                        <div className="mt-3 p-2 bg-blue-50 rounded text-xs">
                          <p className="text-blue-800">
                            ⓘ This partial load can be consolidated with other orders later or dispatched early for time-sensitive deliveries.
                          </p>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>

              <div className="flex justify-end gap-2">
                <Button variant="outline" onClick={() => setShowLoadBuilder(false)}>
                  Cancel
                </Button>
                <Button 
                  className="bg-teal-600 hover:bg-teal-700"
                  disabled={selectedOrders.length === 0 || !newLoad.truckNumber || !newLoad.driverName}
                >
                  <CheckCircle2 className="h-4 w-4 mr-2" />
                  {newLoad.isPartialLoad ? 'Create Partial Load' : 'Create Full Load'}
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>

        {/* Dispatch Dialog */}
        <Dialog open={showDispatchDialog} onOpenChange={setShowDispatchDialog}>
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>Dispatch Load - {selectedLoad?.id}</DialogTitle>
              <DialogDescription>
                Complete the pre-dispatch checklist and dispatch the load to the driver.
              </DialogDescription>
            </DialogHeader>
            {selectedLoad && (
              <div className="space-y-6">
                <div className="p-4 bg-gray-50 rounded-lg">
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <span className="text-muted-foreground">Driver:</span>
                      <p className="font-medium">{selectedLoad.driverName}</p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Truck:</span>
                      <p className="font-medium">{selectedLoad.truckNumber}</p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Total Weight:</span>
                      <p className="font-medium">{selectedLoad.currentWeight.toLocaleString()} lbs</p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">Stops:</span>
                      <p className="font-medium">{selectedLoad.items.length}</p>
                    </div>
                  </div>
                </div>

                <div className="space-y-3">
                  <h3 className="font-medium">Pre-Dispatch Checklist</h3>
                  {[
                    "Load inspection completed",
                    "Load securement verified", 
                    "Driver route briefing completed",
                    "Customer delivery windows confirmed",
                    "Emergency contact information provided",
                    "Vehicle inspection completed"
                  ].map((item, index) => (
                    <div key={index} className="flex items-center gap-3">
                      <Checkbox />
                      <span className="text-sm">{item}</span>
                    </div>
                  ))}
                </div>

                <div>
                  <label className="text-sm font-medium">Dispatch Notes</label>
                  <Textarea 
                    placeholder="Any special instructions for the driver..."
                    rows={3}
                  />
                </div>

                <div className="flex justify-end gap-2">
                  <Button variant="outline" onClick={() => setShowDispatchDialog(false)}>
                    Cancel
                  </Button>
                  <Button className="bg-teal-600 hover:bg-teal-700">
                    <Navigation className="h-4 w-4 mr-2" />
                    Dispatch Load
                  </Button>
                </div>
              </div>
            )}
          </DialogContent>
        </Dialog>
      </Tabs>
    </div>
  );
}