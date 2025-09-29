import { useState } from "react";
import {
  Card,
  CardContent,
  CardHeader,
  CardTitle,
} from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import {
  Tabs,
  TabsContent,
  TabsList,
  TabsTrigger,
} from "./ui/tabs";
import { Progress } from "./ui/progress";
import {
  Dialog,
  DialogContent,
  DialogDescription,
  DialogHeader,
  DialogTitle,
  DialogTrigger,
} from "./ui/dialog";
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
  Info,
} from "lucide-react";
import { StatusChip } from "./StatusChip";
import { LoadCreationSimplified } from "./LoadCreationSimplified";

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
  deliveryStatus?:
    | "pending"
    | "in-progress"
    | "delivered"
    | "failed";
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
  distance: number; // from warehouse
  deliveryRegion:
    | "local"
    | "out-of-town"
    | "island-pool"
    | "okanagan-pool"
    | "customer-pickup";
  regionZone?: string; // specific zone within region
  isPoolDelivery?: boolean;
  poolRoute?: string;
  customerPickupScheduled?: string;
  ferryRequirements?: {
    required: boolean;
    route?: string;
    schedule?: string;
  };
}

interface Load {
  id: string;
  truckNumber: string;
  driverName: string;
  driverPhone: string;
  driverEmail: string;
  trailerType:
    | "flatbed"
    | "enclosed"
    | "step-deck"
    | "dry-van"
    | "refrigerated";
  maxWeight: number;
  maxDimensions: {
    length: number;
    width: number;
    height: number;
  };
  status:
    | "planning"
    | "loading"
    | "loaded"
    | "dispatched"
    | "in-transit"
    | "delivered"
    | "exception";
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
  currentLocation?: {
    lat: number;
    lng: number;
    address: string;
    timestamp: string;
  };
  trackingUpdates: TrackingUpdate[];
  exceptions?: LoadException[];
  deliveryRegion:
    | "local"
    | "out-of-town"
    | "island-pool"
    | "okanagan-pool"
    | "customer-pickup";
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
  type:
    | "delay"
    | "damage"
    | "weather"
    | "breakdown"
    | "customer-issue"
    | "other";
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
  bookingRequired: boolean;
  cutoffTime: string;
}

interface DeliveryRegion {
  id: string;
  name: string;
  type:
    | "local"
    | "out-of-town"
    | "island-pool"
    | "okanagan-pool"
    | "customer-pickup";
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
  };
}

interface Truck {
  id: string;
  number: string;
  type:
    | "flatbed"
    | "enclosed"
    | "step-deck"
    | "dry-van"
    | "refrigerated";
  maxWeight: number;
  maxDimensions: {
    length: number;
    width: number;
    height: number;
  };
  status:
    | "available"
    | "assigned"
    | "maintenance"
    | "out-of-service";
  currentDriver?: string;
  location?: string;
  fuelEfficiency: number; // mpg
  assignedRegion?: string;
  poolTruck?: boolean;
  ferryCapable?: boolean;
}

const truckTypes = [
  "flatbed",
  "enclosed",
  "step-deck",
  "dry-van",
  "refrigerated",
];
const deliveryRegions = [
  "local",
  "out-of-town",
  "island-pool",
  "okanagan-pool",
  "customer-pickup",
];

const regionalData: DeliveryRegion[] = [
  {
    id: "local",
    name: "Local Delivery",
    type: "local",
    description:
      "Same-day and next-day deliveries within metro area",
    coverage: [
      "Vancouver",
      "Burnaby",
      "Richmond",
      "Surrey",
      "Coquitlam",
      "North Vancouver",
    ],
    characteristics: {
      averageDistance: 25,
      deliveryFrequency: "Daily",
      requiresPooling: false,
      ferryDependent: false,
      seasonalAccess: false,
    },
    operationalInfo: {
      coordinator: "Sarah Chen",
      phone: "(604) 555-0123",
      email: "sarah.chen@metalflow.com",
      workingHours: "6:00 AM - 6:00 PM",
      specialRequirements: [
        "Traffic restrictions in downtown core",
        "Dock height limitations",
      ],
    },
    metrics: {
      activeOrders: 23,
      pendingPickups: 0,
      averageDeliveryTime: "4-8 hours",
      utilizationRate: 85,
    },
  },
  {
    id: "out-of-town",
    name: "Multi Out of Town Lanes",
    type: "out-of-town",
    description:
      "Regional deliveries to multiple towns and cities",
    coverage: [
      "Calgary",
      "Edmonton",
      "Saskatoon",
      "Regina",
      "Winnipeg",
      "Kelowna",
    ],
    characteristics: {
      averageDistance: 450,
      deliveryFrequency: "2-3 times weekly",
      requiresPooling: true,
      ferryDependent: false,
      seasonalAccess: true,
    },
    operationalInfo: {
      coordinator: "Mike Rodriguez",
      phone: "(604) 555-0456",
      email: "mike.rodriguez@metalflow.com",
      workingHours: "24/7 Operations",
      specialRequirements: [
        "Winter driving conditions",
        "Cross-border documentation",
        "Fuel planning",
      ],
    },
    metrics: {
      activeOrders: 18,
      pendingPickups: 3,
      averageDeliveryTime: "2-4 days",
      utilizationRate: 72,
    },
  },
  {
    id: "island-pool",
    name: "Island Pool Trucks",
    type: "island-pool",
    description:
      "Consolidated ferry-dependent deliveries to Vancouver Island",
    coverage: [
      "Victoria",
      "Nanaimo",
      "Duncan",
      "Courtenay",
      "Campbell River",
      "Port Alberni",
    ],
    characteristics: {
      averageDistance: 120,
      deliveryFrequency: "Twice weekly",
      requiresPooling: true,
      ferryDependent: true,
      seasonalAccess: false,
    },
    operationalInfo: {
      coordinator: "Jennifer Wilson",
      phone: "(604) 555-0789",
      email: "jennifer.wilson@metalflow.com",
      workingHours: "5:00 AM - 8:00 PM",
      specialRequirements: [
        "Ferry reservations",
        "Tidal schedules",
        "Island weight restrictions",
      ],
    },
    metrics: {
      activeOrders: 12,
      pendingPickups: 8,
      averageDeliveryTime: "3-5 days",
      utilizationRate: 68,
    },
  },
  {
    id: "okanagan-pool",
    name: "Okanagan Pool Trucks",
    type: "okanagan-pool",
    description: "Pooled deliveries to Okanagan Valley region",
    coverage: [
      "Kelowna",
      "Vernon",
      "Penticton",
      "Kamloops",
      "Salmon Arm",
      "Oliver",
    ],
    characteristics: {
      averageDistance: 380,
      deliveryFrequency: "Weekly",
      requiresPooling: true,
      ferryDependent: false,
      seasonalAccess: true,
    },
    operationalInfo: {
      coordinator: "Carlos Martinez",
      phone: "(604) 555-0321",
      email: "carlos.martinez@metalflow.com",
      workingHours: "6:00 AM - 6:00 PM",
      specialRequirements: [
        "Mountain pass conditions",
        "Seasonal road closures",
        "Rural access planning",
      ],
    },
    metrics: {
      activeOrders: 8,
      pendingPickups: 5,
      averageDeliveryTime: "4-7 days",
      utilizationRate: 58,
    },
  },
  {
    id: "customer-pickup",
    name: "Customer Pickup",
    type: "customer-pickup",
    description:
      "Customer self-pickup coordination and scheduling",
    coverage: [
      "Warehouse Dock A",
      "Warehouse Dock B",
      "Will Call Area",
    ],
    characteristics: {
      averageDistance: 0,
      deliveryFrequency: "On-demand",
      requiresPooling: false,
      ferryDependent: false,
      seasonalAccess: false,
    },
    operationalInfo: {
      coordinator: "Lisa Thompson",
      phone: "(604) 555-0654",
      email: "lisa.thompson@metalflow.com",
      workingHours: "7:00 AM - 5:00 PM",
      specialRequirements: [
        "Pickup appointments",
        "Loading equipment",
        "Documentation verification",
      ],
    },
    metrics: {
      activeOrders: 15,
      pendingPickups: 9,
      averageDeliveryTime: "Same day",
      utilizationRate: 95,
    },
  },
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
    ferryCapable: false,
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
    ferryCapable: false,
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
    ferryCapable: true,
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
    ferryCapable: false,
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
    ferryCapable: false,
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
    ferryCapable: true,
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
    ferryCapable: false,
  },
];

// BC Metal, Roofing, Gutters & Window Company Orders - 30+ orders
const availableOrders: AvailableOrder[] = [
  // LOCAL DELIVERY REGION
  {
    id: "ORD-2024-0001",
    pickingListId: "PL-2024-0001",
    customerName: "Pacific Metal Roofing Ltd",
    orderNumber: "PMR-8901",
    destination: "1245 Industrial Way, Vancouver, BC V5L 3C2",
    city: "Vancouver",
    state: "BC",
    province: "BC",
    zipCode: "V5L3C2",
    weight: 12400,
    pieces: 48,
    dimensions: { length: 12, width: 3, height: 2 },
    priority: "high",
    requiredDeliveryDate: "2024-01-22",
    readyDate: "2024-01-20",
    customerContact: "David Chen",
    customerPhone: "(604) 555-7890",
    specialInstructions:
      "Forklift available on site for unloading",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has forklift",
    pickingStatus: "ready",
    distance: 15,
    deliveryRegion: "local",
    regionZone: "Vancouver East",
  },
  {
    id: "ORD-2024-0002",
    pickingListId: "PL-2024-0002",
    customerName: "West Side Windows & Doors",
    orderNumber: "WSW-4567",
    destination: "789 Broadway Ave, Vancouver, BC V5Z 1K5",
    city: "Vancouver",
    state: "BC",
    province: "BC",
    zipCode: "V5Z1K5",
    weight: 8900,
    pieces: 32,
    dimensions: { length: 20, width: 4, height: 1 },
    priority: "urgent",
    requiredDeliveryDate: "2024-01-21",
    readyDate: "2024-01-19",
    customerContact: "Jennifer Wong",
    customerPhone: "(604) 555-3344",
    specialInstructions: "Delivery between 7AM-9AM only",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 12,
    deliveryRegion: "local",
    regionZone: "Vancouver West",
  },
  {
    id: "ORD-2024-0003",
    pickingListId: "PL-2024-0003",
    customerName: "Richmond Gutter Systems",
    orderNumber: "RGS-2234",
    destination: "3456 No. 3 Road, Richmond, BC V6X 2C1",
    city: "Richmond",
    state: "BC",
    province: "BC",
    zipCode: "V6X2C1",
    weight: 6700,
    pieces: 85,
    dimensions: { length: 10, width: 5, height: 1 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-23",
    readyDate: "2024-01-21",
    customerContact: "Mike Thompson",
    customerPhone: "(604) 555-9876",
    specialInstructions: "Call 30 minutes before delivery",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Ground level unloading",
    pickingStatus: "ready",
    distance: 18,
    deliveryRegion: "local",
    regionZone: "Richmond",
  },
  {
    id: "ORD-2024-0004",
    pickingListId: "PL-2024-0004",
    customerName: "Burnaby Metal Crafters",
    orderNumber: "BMC-7788",
    destination: "4567 Hastings St, Burnaby, BC V5C 2K8",
    city: "Burnaby",
    state: "BC",
    province: "BC",
    zipCode: "V5C2K8",
    weight: 15600,
    pieces: 38,
    dimensions: { length: 16, width: 3, height: 2 },
    priority: "high",
    requiredDeliveryDate: "2024-01-24",
    readyDate: "2024-01-22",
    customerContact: "Sarah Mitchell",
    customerPhone: "(604) 555-2468",
    specialInstructions: "Overhead crane available",
    loadingRequirements: "Overhead crane needed",
    unloadingRequirements: "Customer has crane",
    pickingStatus: "ready",
    distance: 22,
    deliveryRegion: "local",
    regionZone: "Burnaby",
  },
  {
    id: "ORD-2024-0005",
    pickingListId: "PL-2024-0005",
    customerName: "Coquitlam Roofing & Aluminum",
    orderNumber: "CRA-5599",
    destination: "2890 Lougheed Hwy, Coquitlam, BC V3B 6J6",
    city: "Coquitlam",
    state: "BC",
    province: "BC",
    zipCode: "V3B6J6",
    weight: 9200,
    pieces: 65,
    dimensions: { length: 12, width: 8, height: 1 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-25",
    readyDate: "2024-01-23",
    customerContact: "Tony Rossi",
    customerPhone: "(604) 555-8765",
    specialInstructions: "Weekend delivery OK",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 35,
    deliveryRegion: "local",
    regionZone: "Tri-Cities",
  },
  {
    id: "ORD-2024-0006",
    pickingListId: "PL-2024-0006",
    customerName: "Surrey Window Manufacturing",
    orderNumber: "SWM-3311",
    destination: "15678 Fraser Hwy, Surrey, BC V3S 2W1",
    city: "Surrey",
    state: "BC",
    province: "BC",
    zipCode: "V3S2W1",
    weight: 11800,
    pieces: 28,
    dimensions: { length: 20, width: 6, height: 2 },
    priority: "high",
    requiredDeliveryDate: "2024-01-26",
    readyDate: "2024-01-24",
    customerContact: "Linda Patel",
    customerPhone: "(604) 555-5432",
    specialInstructions: "Loading dock #3",
    loadingRequirements: "Dock level loading",
    unloadingRequirements: "Dock level unloading",
    pickingStatus: "ready",
    distance: 42,
    deliveryRegion: "local",
    regionZone: "Surrey",
  },
  {
    id: "ORD-2024-0007",
    pickingListId: "PL-2024-0007",
    customerName: "North Van Roofing",
    orderNumber: "NVR-4455",
    destination: "567 Marine Dr, North Vancouver, BC V7M 1B6",
    city: "North Vancouver",
    state: "BC",
    province: "BC",
    zipCode: "V7M1B6",
    weight: 14200,
    pieces: 58,
    dimensions: { length: 10, width: 12, height: 2 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-27",
    readyDate: "2024-01-25",
    customerContact: "Tom Wilson",
    customerPhone: "(604) 555-7788",
    specialInstructions: "Bridge restrictions during rush hour",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 28,
    deliveryRegion: "local",
    regionZone: "North Shore",
  },
  {
    id: "ORD-2024-0008",
    pickingListId: "PL-2024-0008",
    customerName: "Delta Window Specialists",
    orderNumber: "DWS-7766",
    destination: "8901 Scott Rd, Delta, BC V4C 4Y8",
    city: "Delta",
    state: "BC",
    province: "BC",
    zipCode: "V4C4Y8",
    weight: 16800,
    pieces: 42,
    dimensions: { length: 12, width: 8, height: 2 },
    priority: "urgent",
    requiredDeliveryDate: "2024-01-24",
    readyDate: "2024-01-22",
    customerContact: "Emily Chen",
    customerPhone: "(604) 555-4466",
    specialInstructions: "Industrial loading area",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has forklift",
    pickingStatus: "ready",
    distance: 38,
    deliveryRegion: "local",
    regionZone: "Delta",
  },

  // ISLAND POOL REGION
  {
    id: "ORD-2024-0009",
    pickingListId: "PL-2024-0009",
    customerName: "Victoria Island Metals",
    orderNumber: "VIM-6677",
    destination: "1890 Douglas St, Victoria, BC V8T 4K7",
    city: "Victoria",
    state: "BC",
    province: "BC",
    zipCode: "V8T4K7",
    weight: 18200,
    pieces: 42,
    dimensions: { length: 10, width: 6, height: 3 },
    priority: "high",
    requiredDeliveryDate: "2024-01-28",
    readyDate: "2024-01-25",
    customerContact: "Robert MacDonald",
    customerPhone: "(250) 555-7766",
    specialInstructions:
      "Ferry booking required - Tsawwassen to Swartz Bay",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has crane",
    pickingStatus: "ready",
    distance: 115,
    deliveryRegion: "island-pool",
    regionZone: "South Island",
    isPoolDelivery: true,
    poolRoute: "VI-SOUTH-01",
    ferryRequirements: {
      required: true,
      route: "Tsawwassen-Swartz Bay",
      schedule: "Multiple daily sailings",
    },
  },
  {
    id: "ORD-2024-0010",
    pickingListId: "PL-2024-0010",
    customerName: "Nanaimo Metal Works",
    orderNumber: "NMW-4488",
    destination: "2345 Terminal Ave, Nanaimo, BC V9S 4K2",
    city: "Nanaimo",
    state: "BC",
    province: "BC",
    zipCode: "V9S4K2",
    weight: 13400,
    pieces: 55,
    dimensions: { length: 12, width: 12, height: 2 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-29",
    readyDate: "2024-01-26",
    customerContact: "Mark Stevens",
    customerPhone: "(250) 555-8899",
    specialInstructions: "Coordinate with ferry schedule",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 145,
    deliveryRegion: "island-pool",
    regionZone: "Mid Island",
    isPoolDelivery: true,
    poolRoute: "VI-CENTRAL-01",
    ferryRequirements: {
      required: true,
      route: "Horseshoe Bay-Departure Bay",
      schedule: "Every 2 hours",
    },
  },
  {
    id: "ORD-2024-0011",
    pickingListId: "PL-2024-0011",
    customerName: "Duncan Roofing Supply",
    orderNumber: "DRS-9900",
    destination: "567 Trans-Canada Hwy, Duncan, BC V9L 3R5",
    city: "Duncan",
    state: "BC",
    province: "BC",
    zipCode: "V9L3R5",
    weight: 21800,
    pieces: 65,
    dimensions: { length: 12, width: 6, height: 3 },
    priority: "urgent",
    requiredDeliveryDate: "2024-01-27",
    readyDate: "2024-01-24",
    customerContact: "Patricia Wilson",
    customerPhone: "(250) 555-4433",
    specialInstructions: "Rush order - ferry priority booking",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has forklift",
    pickingStatus: "ready",
    distance: 135,
    deliveryRegion: "island-pool",
    regionZone: "South Island",
    isPoolDelivery: true,
    poolRoute: "VI-SOUTH-02",
    ferryRequirements: {
      required: true,
      route: "Tsawwassen-Swartz Bay",
      schedule: "Priority booking",
    },
  },
  {
    id: "ORD-2024-0012",
    pickingListId: "PL-2024-0012",
    customerName: "Courtenay Roofing Materials",
    orderNumber: "CRM-1122",
    destination: "1234 Ryan Rd, Courtenay, BC V9N 3R8",
    city: "Courtenay",
    state: "BC",
    province: "BC",
    zipCode: "V9N3R8",
    weight: 16500,
    pieces: 48,
    dimensions: { length: 8, width: 6, height: 3 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-30",
    readyDate: "2024-01-27",
    customerContact: "James Mitchell",
    customerPhone: "(250) 555-6677",
    specialInstructions:
      "Delivery to job site - rough road access",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Ground level delivery",
    pickingStatus: "ready",
    distance: 165,
    deliveryRegion: "island-pool",
    regionZone: "North Island",
    isPoolDelivery: true,
    poolRoute: "VI-NORTH-01",
    ferryRequirements: {
      required: true,
      route: "Horseshoe Bay-Departure Bay",
      schedule: "Every 2 hours",
    },
  },
  {
    id: "ORD-2024-0013",
    pickingListId: "PL-2024-0013",
    customerName: "Campbell River Building Supply",
    orderNumber: "CRB-5533",
    destination: "2345 Island Hwy, Campbell River, BC V9W 2G8",
    city: "Campbell River",
    state: "BC",
    province: "BC",
    zipCode: "V9W2G8",
    weight: 20600,
    pieces: 48,
    dimensions: { length: 14, width: 6, height: 3 },
    priority: "high",
    requiredDeliveryDate: "2024-02-01",
    readyDate: "2024-01-28",
    customerContact: "Jeff Morrison",
    customerPhone: "(250) 555-5544",
    specialInstructions: "Ferry to northern island",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has crane",
    pickingStatus: "ready",
    distance: 185,
    deliveryRegion: "island-pool",
    regionZone: "North Island",
    isPoolDelivery: true,
    poolRoute: "VI-NORTH-02",
    ferryRequirements: {
      required: true,
      route: "Horseshoe Bay-Departure Bay",
      schedule: "Every 2 hours",
    },
  },
  {
    id: "ORD-2024-0014",
    pickingListId: "PL-2024-0014",
    customerName: "Port Alberni Metals",
    orderNumber: "PAM-8899",
    destination: "3456 3rd Ave, Port Alberni, BC V9Y 4E5",
    city: "Port Alberni",
    state: "BC",
    province: "BC",
    zipCode: "V9Y4E5",
    weight: 18400,
    pieces: 55,
    dimensions: { length: 10, width: 7, height: 3 },
    priority: "normal",
    requiredDeliveryDate: "2024-02-02",
    readyDate: "2024-01-29",
    customerContact: "Sandra Lee",
    customerPhone: "(250) 555-6655",
    specialInstructions: "Mountain highway access",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 155,
    deliveryRegion: "island-pool",
    regionZone: "West Island",
    isPoolDelivery: true,
    poolRoute: "VI-WEST-01",
    ferryRequirements: {
      required: true,
      route: "Horseshoe Bay-Departure Bay",
      schedule: "Every 2 hours",
    },
  },
  {
    id: "ORD-2024-0015",
    pickingListId: "PL-2024-0015",
    customerName: "Langford Metal Roofing",
    orderNumber: "LMR-3344",
    destination: "2890 Millstream Rd, Langford, BC V9B 3S1",
    city: "Langford",
    state: "BC",
    province: "BC",
    zipCode: "V9B3S1",
    weight: 17200,
    pieces: 48,
    dimensions: { length: 12, width: 6, height: 3 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-31",
    readyDate: "2024-01-28",
    customerContact: "Paul McKenzie",
    customerPhone: "(250) 555-9900",
    specialInstructions:
      "Ferry required - Horseshoe Bay to Departure Bay",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has forklift",
    pickingStatus: "ready",
    distance: 125,
    deliveryRegion: "island-pool",
    regionZone: "South Island",
    isPoolDelivery: true,
    poolRoute: "VI-SOUTH-03",
    ferryRequirements: {
      required: true,
      route: "Horseshoe Bay-Departure Bay",
      schedule: "Multiple daily sailings",
    },
  },

  // OKANAGAN POOL REGION
  {
    id: "ORD-2024-0016",
    pickingListId: "PL-2024-0016",
    customerName: "Kelowna Metal Fabricators",
    orderNumber: "KMF-7755",
    destination: "2678 Highway 97 N, Kelowna, BC V1X 4J5",
    city: "Kelowna",
    state: "BC",
    province: "BC",
    zipCode: "V1X4J5",
    weight: 24600,
    pieces: 52,
    dimensions: { length: 20, width: 12, height: 4 },
    priority: "high",
    requiredDeliveryDate: "2024-02-02",
    readyDate: "2024-01-29",
    customerContact: "Michael Zhang",
    customerPhone: "(250) 555-9988",
    specialInstructions:
      "Mountain pass conditions - check weather",
    loadingRequirements: "Overhead crane needed",
    unloadingRequirements: "Customer has crane",
    pickingStatus: "ready",
    distance: 380,
    deliveryRegion: "okanagan-pool",
    regionZone: "Central Okanagan",
    isPoolDelivery: true,
    poolRoute: "OK-CENTRAL-01",
  },
  {
    id: "ORD-2024-0017",
    pickingListId: "PL-2024-0017",
    customerName: "Vernon Roofing Supply",
    orderNumber: "VRS-8866",
    destination: "3456 32nd Ave, Vernon, BC V1T 5L8",
    city: "Vernon",
    state: "BC",
    province: "BC",
    zipCode: "V1T5L8",
    weight: 19200,
    pieces: 78,
    dimensions: { length: 16, width: 8, height: 3 },
    priority: "normal",
    requiredDeliveryDate: "2024-02-03",
    readyDate: "2024-01-30",
    customerContact: "Lisa Kowalski",
    customerPhone: "(250) 555-7744",
    specialInstructions: "Pool truck delivery OK",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 410,
    deliveryRegion: "okanagan-pool",
    regionZone: "North Okanagan",
    isPoolDelivery: true,
    poolRoute: "OK-NORTH-01",
  },
  {
    id: "ORD-2024-0018",
    pickingListId: "PL-2024-0018",
    customerName: "Penticton Metal Systems",
    orderNumber: "PMS-5544",
    destination: "789 Industrial Rd, Penticton, BC V2A 7A1",
    city: "Penticton",
    state: "BC",
    province: "BC",
    zipCode: "V2A7A1",
    weight: 22400,
    pieces: 44,
    dimensions: { length: 16, width: 16, height: 3 },
    priority: "urgent",
    requiredDeliveryDate: "2024-02-01",
    readyDate: "2024-01-28",
    customerContact: "David Kumar",
    customerPhone: "(250) 555-3322",
    specialInstructions: "Rush delivery - customer waiting",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has forklift",
    pickingStatus: "ready",
    distance: 420,
    deliveryRegion: "okanagan-pool",
    regionZone: "South Okanagan",
    isPoolDelivery: true,
    poolRoute: "OK-SOUTH-01",
  },
  {
    id: "ORD-2024-0019",
    pickingListId: "PL-2024-0019",
    customerName: "Kamloops Aluminum Supply",
    orderNumber: "KAS-9977",
    destination: "1234 Columbia St, Kamloops, BC V2C 2V8",
    city: "Kamloops",
    state: "BC",
    province: "BC",
    zipCode: "V2C2V8",
    weight: 17800,
    pieces: 65,
    dimensions: { length: 10, width: 8, height: 3 },
    priority: "high",
    requiredDeliveryDate: "2024-02-04",
    readyDate: "2024-01-31",
    customerContact: "Amanda Foster",
    customerPhone: "(250) 555-8855",
    specialInstructions:
      "Coordinate with Thompson River crossing",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 375,
    deliveryRegion: "okanagan-pool",
    regionZone: "Thompson Region",
    isPoolDelivery: true,
    poolRoute: "OK-THOMPSON-01",
  },
  {
    id: "ORD-2024-0020",
    pickingListId: "PL-2024-0020",
    customerName: "Salmon Arm Roofing",
    orderNumber: "SAR-2211",
    destination:
      "1456 Trans-Canada Hwy, Salmon Arm, BC V1E 4N2",
    city: "Salmon Arm",
    state: "BC",
    province: "BC",
    zipCode: "V1E4N2",
    weight: 15600,
    pieces: 52,
    dimensions: { length: 12, width: 6, height: 3 },
    priority: "normal",
    requiredDeliveryDate: "2024-02-05",
    readyDate: "2024-02-01",
    customerContact: "Brian Murphy",
    customerPhone: "(250) 555-9988",
    specialInstructions: "Highway mountain conditions",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 450,
    deliveryRegion: "okanagan-pool",
    regionZone: "Shuswap",
    isPoolDelivery: true,
    poolRoute: "OK-SHUSWAP-01",
  },
  {
    id: "ORD-2024-0021",
    pickingListId: "PL-2024-0021",
    customerName: "Oliver Metal Supply",
    orderNumber: "OMS-4477",
    destination: "789 Main St, Oliver, BC V0H 1T0",
    city: "Oliver",
    state: "BC",
    province: "BC",
    zipCode: "V0H1T0",
    weight: 12800,
    pieces: 68,
    dimensions: { length: 8, width: 10, height: 2 },
    priority: "high",
    requiredDeliveryDate: "2024-02-03",
    readyDate: "2024-01-30",
    customerContact: "Maria Santos",
    customerPhone: "(250) 555-7766",
    specialInstructions: "Wine country delivery - narrow roads",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Ground level delivery",
    pickingStatus: "ready",
    distance: 465,
    deliveryRegion: "okanagan-pool",
    regionZone: "South Okanagan",
    isPoolDelivery: true,
    poolRoute: "OK-SOUTH-02",
  },

  // CUSTOMER PICKUP
  {
    id: "ORD-2024-0022",
    pickingListId: "PL-2024-0022",
    customerName: "Abbotsford Metal Works",
    orderNumber: "AMW-1199",
    destination: "Will Call - Dock A",
    city: "Abbotsford",
    state: "BC",
    province: "BC",
    zipCode: "V2S7M9",
    weight: 8600,
    pieces: 35,
    dimensions: { length: 8, width: 4, height: 2 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-26",
    readyDate: "2024-01-24",
    customerContact: "Steve Johnson",
    customerPhone: "(604) 555-2211",
    specialInstructions: "Customer pickup - call when ready",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer provides transport",
    pickingStatus: "ready",
    distance: 0,
    deliveryRegion: "customer-pickup",
    regionZone: "Warehouse",
    customerPickupScheduled: "2024-01-26T10:00:00",
  },
  {
    id: "ORD-2024-0023",
    pickingListId: "PL-2024-0023",
    customerName: "Langley Gutter Masters",
    orderNumber: "LGM-6688",
    destination: "Will Call - Dock B",
    city: "Langley",
    state: "BC",
    province: "BC",
    zipCode: "V3A4H9",
    weight: 5400,
    pieces: 48,
    dimensions: { length: 6, width: 6, height: 1 },
    priority: "high",
    requiredDeliveryDate: "2024-01-25",
    readyDate: "2024-01-23",
    customerContact: "Rachel Green",
    customerPhone: "(604) 555-9900",
    specialInstructions: "Pickup scheduled for 10 AM",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer provides transport",
    pickingStatus: "ready",
    distance: 0,
    deliveryRegion: "customer-pickup",
    regionZone: "Warehouse",
    customerPickupScheduled: "2024-01-25T10:00:00",
  },
  {
    id: "ORD-2024-0024",
    pickingListId: "PL-2024-0024",
    customerName: "Fraser Valley Metal Supply",
    orderNumber: "FMS-2233",
    destination: "Will Call - Dock C",
    city: "Abbotsford",
    state: "BC",
    province: "BC",
    zipCode: "V2S7M9",
    weight: 22400,
    pieces: 35,
    dimensions: { length: 20, width: 14, height: 4 },
    priority: "high",
    requiredDeliveryDate: "2024-01-26",
    readyDate: "2024-01-24",
    customerContact: "Ahmed Hassan",
    customerPhone: "(604) 555-8899",
    specialInstructions: "Large order - crane truck pickup",
    loadingRequirements: "Overhead crane required",
    unloadingRequirements: "Customer provides crane truck",
    pickingStatus: "ready",
    distance: 0,
    deliveryRegion: "customer-pickup",
    regionZone: "Warehouse",
    customerPickupScheduled: "2024-01-26T14:00:00",
  },

  // OUT OF TOWN REGION
  {
    id: "ORD-2024-0025",
    pickingListId: "PL-2024-0025",
    customerName: "Prince George Metal Works",
    orderNumber: "PMW-4466",
    destination: "2345 Highway 97 S, Prince George, BC V2N 2S8",
    city: "Prince George",
    state: "BC",
    province: "BC",
    zipCode: "V2N2S8",
    weight: 26800,
    pieces: 44,
    dimensions: { length: 16, width: 12, height: 4 },
    priority: "normal",
    requiredDeliveryDate: "2024-02-08",
    readyDate: "2024-02-03",
    customerContact: "William Fraser",
    customerPhone: "(250) 555-7799",
    specialInstructions:
      "Northern BC delivery - winter conditions",
    loadingRequirements: "Overhead crane needed",
    unloadingRequirements: "Customer has crane",
    pickingStatus: "ready",
    distance: 785,
    deliveryRegion: "out-of-town",
    regionZone: "Northern BC",
    isPoolDelivery: true,
    poolRoute: "BC-NORTH-01",
  },
  {
    id: "ORD-2024-0026",
    pickingListId: "PL-2024-0026",
    customerName: "Terrace Roofing Supply",
    orderNumber: "TRS-8877",
    destination: "4567 Keith Ave, Terrace, BC V8G 1K7",
    city: "Terrace",
    state: "BC",
    province: "BC",
    zipCode: "V8G1K7",
    weight: 19600,
    pieces: 62,
    dimensions: { length: 10, width: 8, height: 3 },
    priority: "high",
    requiredDeliveryDate: "2024-02-10",
    readyDate: "2024-02-05",
    customerContact: "Catherine Morrison",
    customerPhone: "(250) 555-6688",
    specialInstructions:
      "Remote northern delivery - coordinate weather",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 835,
    deliveryRegion: "out-of-town",
    regionZone: "Northwest BC",
    isPoolDelivery: true,
    poolRoute: "BC-NORTHWEST-01",
  },

  // Additional Local Orders
  {
    id: "ORD-2024-0027",
    pickingListId: "PL-2024-0027",
    customerName: "Maple Ridge Window & Metal",
    orderNumber: "MWM-6699",
    destination:
      "2234 Dewdney Trunk Rd, Maple Ridge, BC V2X 3E2",
    city: "Maple Ridge",
    state: "BC",
    province: "BC",
    zipCode: "V2X3E2",
    weight: 11200,
    pieces: 38,
    dimensions: { length: 8, width: 4, height: 2 },
    priority: "urgent",
    requiredDeliveryDate: "2024-01-26",
    readyDate: "2024-01-24",
    customerContact: "Kevin O'Brien",
    customerPhone: "(604) 555-3344",
    specialInstructions:
      "Rural delivery - GPS coordinates provided",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 48,
    deliveryRegion: "local",
    regionZone: "Fraser Valley East",
  },
  {
    id: "ORD-2024-0028",
    pickingListId: "PL-2024-0028",
    customerName: "Port Moody Window & Roofing",
    orderNumber: "PWR-9900",
    destination: "3345 St. Johns St, Port Moody, BC V3H 2C4",
    city: "Port Moody",
    state: "BC",
    province: "BC",
    zipCode: "V3H2C4",
    weight: 9800,
    pieces: 45,
    dimensions: { length: 6, width: 4, height: 2 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-28",
    readyDate: "2024-01-26",
    customerContact: "Jennifer Davis",
    customerPhone: "(604) 555-5566",
    specialInstructions:
      "Steep driveway - small truck preferred",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Ground level delivery",
    pickingStatus: "ready",
    distance: 32,
    deliveryRegion: "local",
    regionZone: "Tri-Cities",
  },
  {
    id: "ORD-2024-0029",
    pickingListId: "PL-2024-0029",
    customerName: "White Rock Metal & Glass",
    orderNumber: "WMG-1122",
    destination: "1567 Marine Dr, White Rock, BC V4B 1C5",
    city: "White Rock",
    state: "BC",
    province: "BC",
    zipCode: "V4B1C5",
    weight: 13600,
    pieces: 56,
    dimensions: { length: 12, width: 6, height: 2 },
    priority: "high",
    requiredDeliveryDate: "2024-01-25",
    readyDate: "2024-01-23",
    customerContact: "Mark Thompson",
    customerPhone: "(604) 555-7788",
    specialInstructions:
      "Waterfront location - tide dependent access",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has forklift",
    pickingStatus: "ready",
    distance: 52,
    deliveryRegion: "local",
    regionZone: "South Fraser",
  },
  {
    id: "ORD-2024-0030",
    pickingListId: "PL-2024-0030",
    customerName: "Chilliwack Gutter Systems",
    orderNumber: "CGS-5566",
    destination: "4567 Yale Rd, Chilliwack, BC V2P 6A6",
    city: "Chilliwack",
    state: "BC",
    province: "BC",
    zipCode: "V2P6A6",
    weight: 8400,
    pieces: 72,
    dimensions: { length: 8, width: 5, height: 1 },
    priority: "urgent",
    requiredDeliveryDate: "2024-01-27",
    readyDate: "2024-01-25",
    customerContact: "Susan Patel",
    customerPhone: "(604) 555-2233",
    specialInstructions:
      "Fraser Valley - agricultural area delivery",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer crew available",
    pickingStatus: "ready",
    distance: 68,
    deliveryRegion: "local",
    regionZone: "Fraser Valley",
  },
];

// Sample Loads for BC Metal Companies - 10+ loads
const sampleLoads: Load[] = [
  {
    id: "LD-2024-0001",
    truckNumber: "T-401",
    driverName: "Mike Rodriguez",
    driverPhone: "(604) 555-1234",
    driverEmail: "mrodriguez@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 48000,
    maxDimensions: { length: 48, width: 8.5, height: 8.5 },
    status: "in-transit",
    priority: "high",
    scheduledPickup: "2024-01-22T08:00:00",
    estimatedDelivery: "2024-01-22T16:30:00",
    actualPickup: "2024-01-22T08:15:00",
    dispatchedAt: "2024-01-22T08:30:00",
    dispatchedBy: "Sarah Chen",
    route: [
      "Pacific Metal Roofing Ltd",
      "Burnaby Metal Crafters",
    ],
    currentWeight: 28000,
    utilizationPercentage: 83,
    totalDistance: 37,
    notes:
      "Local delivery route - both customers have forklift available",
    createdBy: "Sarah Chen",
    createdAt: "2024-01-21T14:30:00Z",
    currentLocation: {
      lat: 49.2827,
      lng: -123.1207,
      address: "Highway 1 near Burnaby, BC",
      timestamp: "2024-01-22T11:30:00Z",
    },
    deliveryRegion: "local",
    trackingUpdates: [
      {
        id: "TU-001",
        timestamp: "2024-01-22T08:15:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Departed warehouse",
        updatedBy: "Mike Rodriguez",
      },
      {
        id: "TU-002",
        timestamp: "2024-01-22T11:30:00Z",
        location: {
          lat: 49.2827,
          lng: -123.1207,
          address: "Highway 1 near Burnaby, BC",
        },
        status: "En route to second delivery",
        notes: "First delivery completed, heading to Burnaby",
        updatedBy: "Mike Rodriguez",
      },
    ],
    items: [
      {
        id: "LI-001-001",
        pickingListId: "PL-2024-0001",
        customerName: "Pacific Metal Roofing Ltd",
        orderNumber: "PMR-8901",
        destination:
          "1245 Industrial Way, Vancouver, BC V5L 3C2",
        weight: 12400,
        pieces: 48,
        dimensions: { length: 12, width: 3, height: 2 },
        priority: "high",
        requiredDeliveryDate: "2024-01-22",
        customerContact: "David Chen",
        customerPhone: "(604) 555-7890",
        specialInstructions:
          "Forklift available on site for unloading",
        unloadingRequirements: "Customer has forklift",
        deliveryStatus: "delivered",
        deliveryTime: "2024-01-22T10:45:00Z",
        signedBy: "David Chen",
        deliveryNotes:
          "Unloaded successfully - galvanized roofing panels",
      },
      {
        id: "LI-001-002",
        pickingListId: "PL-2024-0004",
        customerName: "Burnaby Metal Crafters",
        orderNumber: "BMC-7788",
        destination: "4567 Hastings St, Burnaby, BC V5C 2K8",
        weight: 15600,
        pieces: 38,
        dimensions: { length: 16, width: 3, height: 2 },
        priority: "high",
        requiredDeliveryDate: "2024-01-24",
        customerContact: "Sarah Mitchell",
        customerPhone: "(604) 555-2468",
        unloadingRequirements: "Customer has crane",
        deliveryStatus: "pending",
      },
    ],
  },
  {
    id: "LD-2024-0002",
    truckNumber: "T-312",
    driverName: "Jennifer Wilson",
    driverPhone: "(250) 555-2345",
    driverEmail: "jwilson@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 52000,
    maxDimensions: { length: 50, width: 8.5, height: 11.5 },
    status: "dispatched",
    priority: "high",
    scheduledPickup: "2024-01-28T06:00:00",
    estimatedDelivery: "2024-01-28T18:00:00",
    actualPickup: "2024-01-28T06:15:00",
    dispatchedAt: "2024-01-28T07:00:00",
    dispatchedBy: "Jennifer Wilson",
    route: ["Victoria Island Metals", "Duncan Roofing Supply"],
    currentWeight: 40000,
    utilizationPercentage: 77,
    totalDistance: 250,
    notes:
      "Island pool delivery - ferry booked on Tsawwassen-Swartz Bay",
    createdBy: "Jennifer Wilson",
    createdAt: "2024-01-27T15:00:00Z",
    deliveryRegion: "island-pool",
    regionRoute: {
      region: "Vancouver Island",
      subRegions: ["Victoria", "Duncan"],
      estimatedDays: 1,
      ferryRequired: true,
      poolOptimized: true,
      routePattern: "fixed",
    },
    poolInfo: {
      poolType: "island",
      poolRoute: "VI-SOUTH-01",
      frequency: "daily",
      nextDeparture: "2024-01-28T08:00:00Z",
      capacity: {
        weight: 52000,
        volume: 2600,
        currentUtilization: 77,
      },
      consolidationDeadline: "2024-01-28T07:30:00Z",
      coordinator: "Jennifer Wilson",
      coordinatorPhone: "(604) 555-0789",
    },
    ferrySchedule: [
      {
        route: "Tsawwassen-Swartz Bay",
        departure: "2024-01-28T08:00:00Z",
        arrival: "2024-01-28T09:35:00Z",
        bookingRequired: true,
        cutoffTime: "2024-01-28T07:30:00Z",
      },
    ],
    trackingUpdates: [
      {
        id: "TU-003",
        timestamp: "2024-01-28T06:15:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Departed warehouse",
        updatedBy: "Jennifer Wilson",
      },
      {
        id: "TU-004",
        timestamp: "2024-01-28T07:45:00Z",
        location: {
          lat: 49.0037,
          lng: -123.1336,
          address: "Tsawwassen Ferry Terminal, BC",
        },
        status: "At ferry terminal",
        notes: "Waiting to board ferry",
        updatedBy: "Jennifer Wilson",
      },
    ],
    items: [
      {
        id: "LI-002-001",
        pickingListId: "PL-2024-0009",
        customerName: "Victoria Island Metals",
        orderNumber: "VIM-6677",
        destination: "1890 Douglas St, Victoria, BC V8T 4K7",
        weight: 18200,
        pieces: 42,
        dimensions: { length: 10, width: 6, height: 3 },
        priority: "high",
        requiredDeliveryDate: "2024-01-28",
        customerContact: "Robert MacDonald",
        customerPhone: "(250) 555-7766",
        specialInstructions:
          "Ferry booking required - Tsawwassen to Swartz Bay",
        unloadingRequirements: "Customer has crane",
        deliveryStatus: "pending",
      },
      {
        id: "LI-002-002",
        pickingListId: "PL-2024-0011",
        customerName: "Duncan Roofing Supply",
        orderNumber: "DRS-9900",
        destination: "567 Trans-Canada Hwy, Duncan, BC V9L 3R5",
        weight: 21800,
        pieces: 65,
        dimensions: { length: 12, width: 6, height: 3 },
        priority: "urgent",
        requiredDeliveryDate: "2024-01-27",
        customerContact: "Patricia Wilson",
        customerPhone: "(250) 555-4433",
        specialInstructions:
          "Rush order - ferry priority booking",
        unloadingRequirements: "Customer has forklift",
        deliveryStatus: "pending",
      },
    ],
  },
  {
    id: "LD-2024-0003",
    truckNumber: "T-205",
    driverName: "Carlos Martinez",
    driverPhone: "(250) 555-3456",
    driverEmail: "cmartinez@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 44000,
    maxDimensions: { length: 45, width: 8, height: 9 },
    status: "delivered",
    priority: "normal",
    scheduledPickup: "2024-02-02T07:00:00",
    estimatedDelivery: "2024-02-02T17:00:00",
    actualPickup: "2024-02-02T07:15:00",
    actualDelivery: "2024-02-02T16:45:00",
    dispatchedAt: "2024-02-02T07:30:00",
    dispatchedBy: "Carlos Martinez",
    route: ["Kelowna Metal Fabricators"],
    currentWeight: 24600,
    utilizationPercentage: 56,
    totalDistance: 380,
    notes: "Okanagan delivery - mountain pass conditions good",
    createdBy: "Carlos Martinez",
    createdAt: "2024-02-01T16:15:00Z",
    deliveryRegion: "okanagan-pool",
    regionRoute: {
      region: "Okanagan Valley",
      subRegions: ["Kelowna"],
      estimatedDays: 1,
      ferryRequired: false,
      poolOptimized: true,
      routePattern: "flexible",
    },
    poolInfo: {
      poolType: "okanagan",
      poolRoute: "OK-CENTRAL-01",
      frequency: "weekly",
      nextDeparture: "2024-02-02T07:00:00Z",
      capacity: {
        weight: 44000,
        volume: 2200,
        currentUtilization: 56,
      },
      consolidationDeadline: "2024-02-02T06:00:00Z",
      coordinator: "Carlos Martinez",
      coordinatorPhone: "(604) 555-0321",
    },
    trackingUpdates: [
      {
        id: "TU-005",
        timestamp: "2024-02-02T07:15:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Departed warehouse",
        updatedBy: "Carlos Martinez",
      },
      {
        id: "TU-006",
        timestamp: "2024-02-02T16:45:00Z",
        location: {
          lat: 49.888,
          lng: -119.496,
          address: "Kelowna Metal Fabricators, Kelowna, BC",
        },
        status: "Delivered",
        notes:
          "Delivery completed successfully with customer crane",
        updatedBy: "Carlos Martinez",
      },
    ],
    items: [
      {
        id: "LI-003-001",
        pickingListId: "PL-2024-0016",
        customerName: "Kelowna Metal Fabricators",
        orderNumber: "KMF-7755",
        destination: "2678 Highway 97 N, Kelowna, BC V1X 4J5",
        weight: 24600,
        pieces: 52,
        dimensions: { length: 20, width: 12, height: 4 },
        priority: "high",
        requiredDeliveryDate: "2024-02-02",
        customerContact: "Michael Zhang",
        customerPhone: "(250) 555-9988",
        specialInstructions:
          "Mountain pass conditions - check weather",
        unloadingRequirements: "Customer has crane",
        deliveryStatus: "delivered",
        deliveryTime: "2024-02-02T16:45:00Z",
        signedBy: "Michael Zhang",
        deliveryNotes:
          "Structural steel beams delivered successfully",
      },
    ],
  },
  {
    id: "LD-2024-0004",
    truckNumber: "T-156",
    driverName: "David Thompson",
    driverPhone: "(604) 555-4567",
    driverEmail: "dthompson@metalflow.com",
    trailerType: "dry-van",
    maxWeight: 42000,
    maxDimensions: { length: 40, width: 8, height: 9 },
    status: "loading",
    priority: "high",
    scheduledPickup: "2024-01-24T09:00:00",
    route: [
      "Delta Window Specialists",
      "Surrey Window Manufacturing",
    ],
    currentWeight: 28600,
    utilizationPercentage: 68,
    totalDistance: 80,
    notes:
      "Local window delivery - enclosed trailer for weather protection",
    createdBy: "David Thompson",
    createdAt: "2024-01-23T14:00:00Z",
    deliveryRegion: "local",
    trackingUpdates: [
      {
        id: "TU-007",
        timestamp: "2024-01-24T08:30:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Loading in progress",
        notes: "Loading window frames and glazing systems",
        updatedBy: "David Thompson",
      },
    ],
    items: [
      {
        id: "LI-004-001",
        pickingListId: "PL-2024-0008",
        customerName: "Delta Window Specialists",
        orderNumber: "DWS-7766",
        destination: "8901 Scott Rd, Delta, BC V4C 4Y8",
        weight: 16800,
        pieces: 42,
        dimensions: { length: 12, width: 8, height: 2 },
        priority: "urgent",
        requiredDeliveryDate: "2024-01-24",
        customerContact: "Emily Chen",
        customerPhone: "(604) 555-4466",
        specialInstructions: "Industrial loading area",
        unloadingRequirements: "Customer has forklift",
        deliveryStatus: "pending",
      },
      {
        id: "LI-004-002",
        pickingListId: "PL-2024-0006",
        customerName: "Surrey Window Manufacturing",
        orderNumber: "SWM-3311",
        destination: "15678 Fraser Hwy, Surrey, BC V3S 2W1",
        weight: 11800,
        pieces: 28,
        dimensions: { length: 20, width: 6, height: 2 },
        priority: "high",
        requiredDeliveryDate: "2024-01-26",
        customerContact: "Linda Patel",
        customerPhone: "(604) 555-5432",
        specialInstructions: "Loading dock #3",
        unloadingRequirements: "Dock level unloading",
        deliveryStatus: "pending",
      },
    ],
  },
  {
    id: "LD-2024-0005",
    truckNumber: "T-789",
    driverName: "Lisa Rodriguez",
    driverPhone: "(604) 555-5678",
    driverEmail: "lrodriguez@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 46000,
    maxDimensions: { length: 48, width: 8.5, height: 8.5 },
    status: "planning",
    priority: "normal",
    scheduledPickup: "2024-01-25T08:00:00",
    route: [
      "Richmond Gutter Systems",
      "Coquitlam Roofing & Aluminum",
    ],
    currentWeight: 15900,
    utilizationPercentage: 35,
    totalDistance: 53,
    notes:
      "Gutter system delivery - lightweight but bulky items",
    createdBy: "Lisa Rodriguez",
    createdAt: "2024-01-24T16:00:00Z",
    deliveryRegion: "local",
    trackingUpdates: [],
    items: [
      {
        id: "LI-005-001",
        pickingListId: "PL-2024-0003",
        customerName: "Richmond Gutter Systems",
        orderNumber: "RGS-2234",
        destination: "3456 No. 3 Road, Richmond, BC V6X 2C1",
        weight: 6700,
        pieces: 85,
        dimensions: { length: 10, width: 5, height: 1 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-23",
        customerContact: "Mike Thompson",
        customerPhone: "(604) 555-9876",
        specialInstructions: "Call 30 minutes before delivery",
        unloadingRequirements: "Ground level unloading",
        deliveryStatus: "pending",
      },
      {
        id: "LI-005-002",
        pickingListId: "PL-2024-0005",
        customerName: "Coquitlam Roofing & Aluminum",
        orderNumber: "CRA-5599",
        destination: "2890 Lougheed Hwy, Coquitlam, BC V3B 6J6",
        weight: 9200,
        pieces: 65,
        dimensions: { length: 12, width: 8, height: 1 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-25",
        customerContact: "Tony Rossi",
        customerPhone: "(604) 555-8765",
        specialInstructions: "Weekend delivery OK",
        unloadingRequirements: "Customer crew available",
        deliveryStatus: "pending",
      },
    ],
  },
  {
    id: "LD-2024-0006",
    truckNumber: "T-098",
    driverName: "Amanda Foster",
    driverPhone: "(250) 555-6789",
    driverEmail: "afoster@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 40000,
    maxDimensions: { length: 40, width: 8, height: 9 },
    status: "in-transit",
    priority: "high",
    scheduledPickup: "2024-02-04T06:00:00",
    estimatedDelivery: "2024-02-04T16:00:00",
    actualPickup: "2024-02-04T06:30:00",
    dispatchedAt: "2024-02-04T07:00:00",
    dispatchedBy: "Amanda Foster",
    route: [
      "Kamloops Aluminum Supply",
      "Vernon Roofing Supply",
    ],
    currentWeight: 37000,
    utilizationPercentage: 93,
    totalDistance: 835,
    notes: "Okanagan pool - high utilization load",
    createdBy: "Amanda Foster",
    createdAt: "2024-02-03T15:30:00Z",
    deliveryRegion: "okanagan-pool",
    regionRoute: {
      region: "Okanagan Valley",
      subRegions: ["Kamloops", "Vernon"],
      estimatedDays: 1,
      ferryRequired: false,
      poolOptimized: true,
      routePattern: "fixed",
    },
    poolInfo: {
      poolType: "okanagan",
      poolRoute: "OK-THOMPSON-01",
      frequency: "weekly",
      nextDeparture: "2024-02-04T06:00:00Z",
      capacity: {
        weight: 40000,
        volume: 2000,
        currentUtilization: 93,
      },
      consolidationDeadline: "2024-02-04T05:00:00Z",
      coordinator: "Amanda Foster",
      coordinatorPhone: "(250) 555-8855",
    },
    currentLocation: {
      lat: 50.6745,
      lng: -120.3273,
      address: "Highway 5 near Kamloops, BC",
      timestamp: "2024-02-04T12:00:00Z",
    },
    trackingUpdates: [
      {
        id: "TU-008",
        timestamp: "2024-02-04T06:30:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Departed warehouse",
        updatedBy: "Amanda Foster",
      },
      {
        id: "TU-009",
        timestamp: "2024-02-04T12:00:00Z",
        location: {
          lat: 50.6745,
          lng: -120.3273,
          address: "Highway 5 near Kamloops, BC",
        },
        status: "En route to first delivery",
        notes: "Good road conditions, on schedule",
        updatedBy: "Amanda Foster",
      },
    ],
    items: [
      {
        id: "LI-006-001",
        pickingListId: "PL-2024-0019",
        customerName: "Kamloops Aluminum Supply",
        orderNumber: "KAS-9977",
        destination: "1234 Columbia St, Kamloops, BC V2C 2V8",
        weight: 17800,
        pieces: 65,
        dimensions: { length: 10, width: 8, height: 3 },
        priority: "high",
        requiredDeliveryDate: "2024-02-04",
        customerContact: "Amanda Foster",
        customerPhone: "(250) 555-8855",
        specialInstructions:
          "Coordinate with Thompson River crossing",
        unloadingRequirements: "Customer crew available",
        deliveryStatus: "pending",
      },
      {
        id: "LI-006-002",
        pickingListId: "PL-2024-0017",
        customerName: "Vernon Roofing Supply",
        orderNumber: "VRS-8866",
        destination: "3456 32nd Ave, Vernon, BC V1T 5L8",
        weight: 19200,
        pieces: 78,
        dimensions: { length: 16, width: 8, height: 3 },
        priority: "normal",
        requiredDeliveryDate: "2024-02-03",
        customerContact: "Lisa Kowalski",
        customerPhone: "(250) 555-7744",
        specialInstructions: "Pool truck delivery OK",
        unloadingRequirements: "Customer crew available",
        deliveryStatus: "pending",
      },
    ],
  },
  {
    id: "LD-2024-0007",
    truckNumber: "T-432",
    driverName: "Robert Singh",
    driverPhone: "(604) 555-7890",
    driverEmail: "rsingh@metalflow.com",
    trailerType: "dry-van",
    maxWeight: 44000,
    maxDimensions: { length: 45, width: 8, height: 9 },
    status: "exception",
    priority: "urgent",
    scheduledPickup: "2024-01-27T08:00:00",
    estimatedDelivery: "2024-01-27T17:00:00",
    actualPickup: "2024-01-27T08:30:00",
    dispatchedAt: "2024-01-27T09:00:00",
    dispatchedBy: "Robert Singh",
    route: [
      "Chilliwack Gutter Systems",
      "Mission Ridge Windows",
    ],
    currentWeight: 23200,
    utilizationPercentage: 53,
    totalDistance: 143,
    notes: "Fraser Valley delivery - weather delay",
    createdBy: "Robert Singh",
    createdAt: "2024-01-26T16:30:00Z",
    deliveryRegion: "local",
    currentLocation: {
      lat: 49.1628,
      lng: -121.9513,
      address: "Highway 1 near Chilliwack, BC",
      timestamp: "2024-01-27T14:30:00Z",
    },
    exceptions: [
      {
        id: "EX-001",
        type: "weather",
        severity: "medium",
        description:
          "Heavy rain causing delivery delays in Fraser Valley",
        reportedAt: "2024-01-27T13:00:00Z",
        reportedBy: "Robert Singh",
        resolved: false,
      },
    ],
    trackingUpdates: [
      {
        id: "TU-010",
        timestamp: "2024-01-27T08:30:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Departed warehouse",
        updatedBy: "Robert Singh",
      },
      {
        id: "TU-011",
        timestamp: "2024-01-27T14:30:00Z",
        location: {
          lat: 49.1628,
          lng: -121.9513,
          address: "Highway 1 near Chilliwack, BC",
        },
        status: "Delayed due to weather",
        notes: "Heavy rain, proceeding with caution",
        updatedBy: "Robert Singh",
      },
    ],
    items: [
      {
        id: "LI-007-001",
        pickingListId: "PL-2024-0030",
        customerName: "Chilliwack Gutter Systems",
        orderNumber: "CGS-5566",
        destination: "4567 Yale Rd, Chilliwack, BC V2P 6A6",
        weight: 8400,
        pieces: 72,
        dimensions: { length: 8, width: 5, height: 1 },
        priority: "urgent",
        requiredDeliveryDate: "2024-01-27",
        customerContact: "Susan Patel",
        customerPhone: "(604) 555-2233",
        specialInstructions:
          "Fraser Valley - agricultural area delivery",
        unloadingRequirements: "Customer crew available",
        deliveryStatus: "pending",
      },
      {
        id: "LI-007-002",
        pickingListId: "PL-2024-0028",
        customerName: "Mission Ridge Windows",
        orderNumber: "MRW-7788",
        destination: "1789 Lougheed Hwy, Mission, BC V2V 2P5",
        weight: 14800,
        pieces: 42,
        dimensions: { length: 10, width: 5, height: 2 },
        priority: "high",
        requiredDeliveryDate: "2024-01-29",
        customerContact: "Robert Chang",
        customerPhone: "(604) 555-4455",
        specialInstructions:
          "Industrial park - loading dock available",
        unloadingRequirements: "Dock level unloading",
        deliveryStatus: "pending",
      },
    ],
  },
  {
    id: "LD-2024-0008",
    truckNumber: "T-567",
    driverName: "Maria Santos",
    driverPhone: "(250) 555-8901",
    driverEmail: "msantos@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 48000,
    maxDimensions: { length: 48, width: 8.5, height: 8.5 },
    status: "dispatched",
    priority: "normal",
    scheduledPickup: "2024-02-08T05:00:00",
    estimatedDelivery: "2024-02-08T19:00:00",
    actualPickup: "2024-02-08T05:15:00",
    dispatchedAt: "2024-02-08T05:30:00",
    dispatchedBy: "Maria Santos",
    route: ["Prince George Metal Works"],
    currentWeight: 26800,
    utilizationPercentage: 56,
    totalDistance: 785,
    notes: "Northern BC delivery - winter driving conditions",
    createdBy: "Maria Santos",
    createdAt: "2024-02-07T14:00:00Z",
    deliveryRegion: "out-of-town",
    regionRoute: {
      region: "Northern BC",
      subRegions: ["Prince George"],
      estimatedDays: 1,
      ferryRequired: false,
      poolOptimized: true,
      routePattern: "fixed",
    },
    poolInfo: {
      poolType: "multi-town",
      poolRoute: "BC-NORTH-01",
      frequency: "weekly",
      nextDeparture: "2024-02-08T05:00:00Z",
      capacity: {
        weight: 48000,
        volume: 2400,
        currentUtilization: 56,
      },
      consolidationDeadline: "2024-02-08T04:00:00Z",
      coordinator: "Maria Santos",
      coordinatorPhone: "(250) 555-7799",
    },
    trackingUpdates: [
      {
        id: "TU-012",
        timestamp: "2024-02-08T05:15:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Departed warehouse",
        notes: "Heading north on Highway 97",
        updatedBy: "Maria Santos",
      },
    ],
    items: [
      {
        id: "LI-008-001",
        pickingListId: "PL-2024-0025",
        customerName: "Prince George Metal Works",
        orderNumber: "PMW-4466",
        destination:
          "2345 Highway 97 S, Prince George, BC V2N 2S8",
        weight: 26800,
        pieces: 44,
        dimensions: { length: 16, width: 12, height: 4 },
        priority: "normal",
        requiredDeliveryDate: "2024-02-08",
        customerContact: "William Fraser",
        customerPhone: "(250) 555-7799",
        specialInstructions:
          "Northern BC delivery - winter conditions",
        unloadingRequirements: "Customer has crane",
        deliveryStatus: "pending",
      },
    ],
  },
  {
    id: "LD-2024-0009",
    truckNumber: "T-223",
    driverName: "Kevin O'Brien",
    driverPhone: "(604) 555-9012",
    driverEmail: "kobrien@metalflow.com",
    trailerType: "enclosed",
    maxWeight: 42000,
    maxDimensions: { length: 40, width: 8, height: 9 },
    status: "delivered",
    priority: "high",
    scheduledPickup: "2024-01-25T09:00:00",
    estimatedDelivery: "2024-01-25T18:00:00",
    actualPickup: "2024-01-25T09:15:00",
    actualDelivery: "2024-01-25T17:30:00",
    dispatchedAt: "2024-01-25T09:30:00",
    dispatchedBy: "Kevin O'Brien",
    route: ["White Rock Metal & Glass", "North Van Roofing"],
    currentWeight: 27800,
    utilizationPercentage: 66,
    totalDistance: 80,
    notes:
      "High-end architectural metal delivery - enclosed trailer",
    createdBy: "Kevin O'Brien",
    createdAt: "2024-01-24T15:45:00Z",
    deliveryRegion: "local",
    trackingUpdates: [
      {
        id: "TU-013",
        timestamp: "2024-01-25T09:15:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Departed warehouse",
        updatedBy: "Kevin O'Brien",
      },
      {
        id: "TU-014",
        timestamp: "2024-01-25T17:30:00Z",
        location: {
          lat: 49.326,
          lng: -123.0781,
          address: "North Van Roofing, North Vancouver, BC",
        },
        status: "All deliveries completed",
        notes: "Both deliveries successful",
        updatedBy: "Kevin O'Brien",
      },
    ],
    items: [
      {
        id: "LI-009-001",
        pickingListId: "PL-2024-0029",
        customerName: "White Rock Metal & Glass",
        orderNumber: "WMG-1122",
        destination: "1567 Marine Dr, White Rock, BC V4B 1C5",
        weight: 13600,
        pieces: 56,
        dimensions: { length: 12, width: 6, height: 2 },
        priority: "high",
        requiredDeliveryDate: "2024-01-25",
        customerContact: "Mark Thompson",
        customerPhone: "(604) 555-7788",
        specialInstructions:
          "Waterfront location - tide dependent access",
        unloadingRequirements: "Customer has forklift",
        deliveryStatus: "delivered",
        deliveryTime: "2024-01-25T14:15:00Z",
        signedBy: "Mark Thompson",
        deliveryNotes:
          "Curtain wall system delivered to waterfront project",
      },
      {
        id: "LI-009-002",
        pickingListId: "PL-2024-0007",
        customerName: "North Van Roofing",
        orderNumber: "NVR-4455",
        destination:
          "567 Marine Dr, North Vancouver, BC V7M 1B6",
        weight: 14200,
        pieces: 58,
        dimensions: { length: 10, width: 12, height: 2 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-27",
        customerContact: "Tom Wilson",
        customerPhone: "(604) 555-7788",
        specialInstructions:
          "Bridge restrictions during rush hour",
        unloadingRequirements: "Customer crew available",
        deliveryStatus: "delivered",
        deliveryTime: "2024-01-25T17:30:00Z",
        signedBy: "Tom Wilson",
        deliveryNotes:
          "Architectural panels delivered for luxury residential project",
      },
    ],
  },
  {
    id: "LD-2024-0010",
    truckNumber: "T-345",
    driverName: "Jennifer Davis",
    driverPhone: "(604) 555-0123",
    driverEmail: "jdavis@metalflow.com",
    trailerType: "flatbed",
    maxWeight: 46000,
    maxDimensions: { length: 48, width: 8.5, height: 8.5 },
    status: "loaded",
    priority: "normal",
    scheduledPickup: "2024-01-30T08:00:00",
    route: ["Tri-Cities Siding", "Port Moody Window & Roofing"],
    currentWeight: 20400,
    utilizationPercentage: 44,
    totalDistance: 74,
    notes: "Tri-Cities area delivery - residential materials",
    createdBy: "Jennifer Davis",
    createdAt: "2024-01-29T16:00:00Z",
    deliveryRegion: "local",
    trackingUpdates: [
      {
        id: "TU-015",
        timestamp: "2024-01-30T07:30:00Z",
        location: {
          lat: 49.2699,
          lng: -123.1,
          address: "MetalFlow Warehouse, Vancouver, BC",
        },
        status: "Loaded and ready for dispatch",
        notes: "All items secured for delivery",
        updatedBy: "Jennifer Davis",
      },
    ],
    items: [
      {
        id: "LI-010-001",
        pickingListId: "PL-2024-0029",
        customerName: "Tri-Cities Siding",
        orderNumber: "TCS-9911",
        destination: "3123 Pinetree Way, Coquitlam, BC V3B 7X3",
        weight: 10600,
        pieces: 58,
        dimensions: { length: 12, width: 3, height: 2 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-30",
        customerContact: "Lisa Rodriguez",
        customerPhone: "(604) 555-6677",
        specialInstructions:
          "Residential area - small truck access only",
        unloadingRequirements: "Ground level delivery",
        deliveryStatus: "pending",
      },
      {
        id: "LI-010-002",
        pickingListId: "PL-2024-0028",
        customerName: "Port Moody Window & Roofing",
        orderNumber: "PWR-9900",
        destination:
          "3345 St. Johns St, Port Moody, BC V3H 2C4",
        weight: 9800,
        pieces: 45,
        dimensions: { length: 6, width: 4, height: 2 },
        priority: "normal",
        requiredDeliveryDate: "2024-01-28",
        customerContact: "Jennifer Davis",
        customerPhone: "(604) 555-5566",
        specialInstructions:
          "Steep driveway - small truck preferred",
        unloadingRequirements: "Ground level delivery",
        deliveryStatus: "pending",
      },
    ],
  },
];

const getStatusColor = (status: string) => {
  switch (status) {
    case "planning":
      return "bg-gray-100 text-gray-800 border-gray-200";
    case "loading":
      return "bg-yellow-100 text-yellow-800 border-yellow-200";
    case "loaded":
      return "bg-blue-100 text-blue-800 border-blue-200";
    case "dispatched":
      return "bg-purple-100 text-purple-800 border-purple-200";
    case "in-transit":
      return "bg-orange-100 text-orange-800 border-orange-200";
    case "delivered":
      return "bg-green-100 text-green-800 border-green-200";
    default:
      return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

const getPriorityColor = (priority: string) => {
  switch (priority) {
    case "urgent":
      return "bg-red-100 text-red-800 border-red-200";
    case "high":
      return "bg-orange-100 text-orange-800 border-orange-200";
    case "normal":
      return "bg-blue-100 text-blue-800 border-blue-200";
    case "low":
      return "bg-gray-100 text-gray-800 border-gray-200";
    default:
      return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Shipping() {
  const [activeTab, setActiveTab] = useState("regional");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [selectedTrailerType, setSelectedTrailerType] =
    useState("all");
  const [selectedRegion, setSelectedRegion] = useState("all");
  const [showLoadBuilder, setShowLoadBuilder] = useState(false);
  const [showDispatchDialog, setShowDispatchDialog] =
    useState(false);
  const [showTrackingDialog, setShowTrackingDialog] =
    useState(false);
  const [selectedLoad, setSelectedLoad] = useState<Load | null>(
    null,
  );
  const [newLoad, setNewLoad] = useState({
    truckNumber: "",
    driverName: "",
    driverPhone: "",
    driverEmail: "",
    trailerType: "flatbed" as const,
    scheduledPickup: "",
    notes: "",
    isPartialLoad: false,
    maxUtilization: 100,
  });
  const [selectedOrders, setSelectedOrders] = useState<
    string[]
  >([]);
  const [loadPlannerFilters, setLoadPlannerFilters] = useState({
    priority: "all",
    destination: "all",
    status: "all",
  });

  const filteredLoads = sampleLoads.filter((load) => {
    const matchesSearch =
      load.id
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      load.truckNumber
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      load.driverName
        .toLowerCase()
        .includes(searchTerm.toLowerCase()) ||
      load.items.some(
        (item) =>
          item.customerName
            .toLowerCase()
            .includes(searchTerm.toLowerCase()) ||
          item.orderNumber
            .toLowerCase()
            .includes(searchTerm.toLowerCase()),
      );
    const matchesStatus =
      selectedStatus === "all" ||
      load.status === selectedStatus;
    const matchesTrailerType =
      selectedTrailerType === "all" ||
      load.trailerType === selectedTrailerType;
    return matchesSearch && matchesStatus && matchesTrailerType;
  });

  return (
    <div className="space-y-6">
      <div>
        <h1>Loads & Shipping</h1>
        <p className="text-muted-foreground">
          Manage truck loads and shipping operations
        </p>
      </div>

      <Tabs
        value={activeTab}
        onValueChange={setActiveTab}
        className="space-y-6"
      >
        <TabsList className="grid w-full grid-cols-7">
          <TabsTrigger
            value="regional"
            className="flex items-center gap-2"
          >
            <MapPin className="h-4 w-4" />
            Regional Overview
          </TabsTrigger>
          <TabsTrigger
            value="loads"
            className="flex items-center gap-2"
          >
            <Truck className="h-4 w-4" />
            Load Management
          </TabsTrigger>
          <TabsTrigger
            value="simple-planning"
            className="flex items-center gap-2"
          >
            <Plus className="h-4 w-4" />
            Simple Planning
          </TabsTrigger>
          <TabsTrigger
            value="pools"
            className="flex items-center gap-2"
          >
            <Layers className="h-4 w-4" />
            Pool Management
          </TabsTrigger>
          <TabsTrigger
            value="execution"
            className="flex items-center gap-2"
          >
            <Play className="h-4 w-4" />
            Shipping Execution
          </TabsTrigger>
          <TabsTrigger
            value="tracking"
            className="flex items-center gap-2"
          >
            <Locate className="h-4 w-4" />
            Live Tracking
          </TabsTrigger>
          <TabsTrigger
            value="analytics"
            className="flex items-center gap-2"
          >
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
            <Select
              value={selectedStatus}
              onValueChange={setSelectedStatus}
            >
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="planning">
                  Planning
                </SelectItem>
                <SelectItem value="loading">Loading</SelectItem>
                <SelectItem value="loaded">Loaded</SelectItem>
                <SelectItem value="dispatched">
                  Dispatched
                </SelectItem>
                <SelectItem value="in-transit">
                  In Transit
                </SelectItem>
                <SelectItem value="delivered">
                  Delivered
                </SelectItem>
              </SelectContent>
            </Select>
            <Select
              value={selectedTrailerType}
              onValueChange={setSelectedTrailerType}
            >
              <SelectTrigger className="w-36">
                <SelectValue placeholder="Trailer type" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Types</SelectItem>
                {truckTypes.map((type) => (
                  <SelectItem
                    key={type}
                    value={type}
                    className="capitalize"
                  >
                    {type.replace("-", " ")}
                  </SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select
              value={selectedRegion}
              onValueChange={setSelectedRegion}
            >
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Delivery region" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Regions</SelectItem>
                <SelectItem value="local">Local</SelectItem>
                <SelectItem value="out-of-town">
                  Out of Town
                </SelectItem>
                <SelectItem value="island-pool">
                  Island Pool
                </SelectItem>
                <SelectItem value="okanagan-pool">
                  Okanagan Pool
                </SelectItem>
                <SelectItem value="customer-pickup">
                  Customer Pickup
                </SelectItem>
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
              <h2 className="text-xl font-semibold">
                Regional Delivery Management
              </h2>
              <p className="text-muted-foreground">
                Manage deliveries across different geographical
                regions and service types
              </p>
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
            {regionalData.map((region) => (
              <Card
                key={region.id}
                className={`cursor-pointer transition-all hover:shadow-md ${
                  region.type === "local"
                    ? "border-green-200 bg-green-50"
                    : region.type === "out-of-town"
                      ? "border-blue-200 bg-blue-50"
                      : region.type === "island-pool"
                        ? "border-purple-200 bg-purple-50"
                        : region.type === "okanagan-pool"
                          ? "border-orange-200 bg-orange-50"
                          : "border-gray-200 bg-gray-50"
                }`}
                onClick={() => setActiveTab("simple-planning")}
              >
                <CardContent className="pt-4">
                  <div className="space-y-3">
                    <div className="flex items-center justify-between">
                      <div
                        className={`p-2 rounded-full ${
                          region.type === "local"
                            ? "bg-green-100"
                            : region.type === "out-of-town"
                              ? "bg-blue-100"
                              : region.type === "island-pool"
                                ? "bg-purple-100"
                                : region.type ===
                                    "okanagan-pool"
                                  ? "bg-orange-100"
                                  : "bg-gray-100"
                        }`}
                      >
                        {region.type === "local" ? (
                          <Building className="h-4 w-4 text-green-600" />
                        ) : region.type === "out-of-town" ? (
                          <Route className="h-4 w-4 text-blue-600" />
                        ) : region.type === "island-pool" ? (
                          <Navigation className="h-4 w-4 text-purple-600" />
                        ) : region.type === "okanagan-pool" ? (
                          <MapPin className="h-4 w-4 text-orange-600" />
                        ) : (
                          <Users className="h-4 w-4 text-gray-600" />
                        )}
                      </div>
                      <Badge
                        variant="outline"
                        className="text-xs"
                      >
                        {region.metrics.utilizationRate}%
                      </Badge>
                    </div>

                    <div>
                      <h3 className="font-medium text-sm">
                        {region.name}
                      </h3>
                      <p className="text-xs text-muted-foreground">
                        {region.description}
                      </p>
                    </div>

                    <div className="space-y-2">
                      <div className="flex justify-between text-xs">
                        <span className="text-muted-foreground">
                          Active Orders:
                        </span>
                        <span className="font-medium">
                          {region.metrics.activeOrders}
                        </span>
                      </div>
                      <div className="flex justify-between text-xs">
                        <span className="text-muted-foreground">
                          Avg Delivery:
                        </span>
                        <span className="font-medium">
                          {region.metrics.averageDeliveryTime}
                        </span>
                      </div>

                      {region.metrics.pendingPickups > 0 && (
                        <div className="flex justify-between text-xs">
                          <span className="text-muted-foreground">
                            Pending Pickups:
                          </span>
                          <Badge
                            variant="destructive"
                            className="text-xs"
                          >
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
                  {regionalData.map((region) => {
                    const regionOrders = availableOrders.filter(
                      (order) =>
                        order.deliveryRegion === region.type,
                    );
                    return (
                      <div
                        key={region.id}
                        className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                      >
                        <div className="flex items-center gap-3">
                          <div
                            className={`w-3 h-3 rounded-full ${
                              region.type === "local"
                                ? "bg-green-500"
                                : region.type === "out-of-town"
                                  ? "bg-blue-500"
                                  : region.type ===
                                      "island-pool"
                                    ? "bg-purple-500"
                                    : region.type ===
                                        "okanagan-pool"
                                      ? "bg-orange-500"
                                      : "bg-gray-500"
                            }`}
                          />
                          <div>
                            <p className="font-medium text-sm">
                              {region.name}
                            </p>
                            <p className="text-xs text-muted-foreground">
                              {regionOrders
                                .reduce(
                                  (sum, order) =>
                                    sum + order.weight,
                                  0,
                                )
                                .toLocaleString()}{" "}
                              lbs
                            </p>
                          </div>
                        </div>
                        <div className="text-right">
                          <Badge variant="outline">
                            {regionOrders.length} orders
                          </Badge>
                          <p className="text-xs text-muted-foreground mt-1">
                            {regionOrders
                              .reduce(
                                (sum, order) =>
                                  sum + order.weight,
                                0,
                              )
                              .toLocaleString()}{" "}
                            lbs
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
                  {regionalData.map((region) => {
                    const regionTrucks = availableTrucks.filter(
                      (truck) =>
                        truck.assignedRegion === region.type,
                    );
                    const activeTrucks = regionTrucks.filter(
                      (truck) =>
                        truck.status !== "maintenance" &&
                        truck.status !== "out-of-service",
                    );
                    return (
                      <div
                        key={region.id}
                        className="flex items-center justify-between p-3 bg-gray-50 rounded-lg"
                      >
                        <div className="flex items-center gap-3">
                          <div
                            className={`w-3 h-3 rounded-full ${
                              region.type === "local"
                                ? "bg-green-500"
                                : region.type === "out-of-town"
                                  ? "bg-blue-500"
                                  : region.type ===
                                      "island-pool"
                                    ? "bg-purple-500"
                                    : region.type ===
                                        "okanagan-pool"
                                      ? "bg-orange-500"
                                      : "bg-gray-500"
                            }`}
                          />
                          <div>
                            <p className="font-medium text-sm">
                              {region.name}
                            </p>
                            <p className="text-xs text-muted-foreground">
                              {activeTrucks.length} active •{" "}
                              {
                                regionTrucks.filter(
                                  (t) => t.poolTruck,
                                ).length
                              }{" "}
                              pool trucks
                            </p>
                          </div>
                        </div>
                        <div className="text-right">
                          <Badge variant="outline">
                            {regionTrucks.length} trucks
                          </Badge>
                          {region.characteristics
                            .ferryDependent && (
                            <p className="text-xs text-blue-600 mt-1">
                              {
                                regionTrucks.filter(
                                  (t) => t.ferryCapable,
                                ).length
                              }{" "}
                              ferry capable
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
                    <p className="font-medium text-yellow-800">
                      Island Pool Alert
                    </p>
                    <p className="text-sm text-yellow-700">
                      Ferry booking required for 8 orders by 4
                      PM today
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
                    <p className="font-medium text-blue-800">
                      Okanagan Pool Ready
                    </p>
                    <p className="text-sm text-blue-700">
                      Pool consolidation complete - ready for
                      dispatch
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
                    <p className="font-medium text-green-800">
                      Customer Pickup
                    </p>
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
                    <p className="text-sm text-muted-foreground">
                      Active Loads
                    </p>
                    <p className="text-2xl font-medium">
                      {
                        sampleLoads.filter((load) =>
                          [
                            "planning",
                            "loading",
                            "loaded",
                            "dispatched",
                          ].includes(load.status),
                        ).length
                      }
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
                    <p className="text-sm text-muted-foreground">
                      In Transit
                    </p>
                    <p className="text-2xl font-medium">
                      {
                        sampleLoads.filter(
                          (load) =>
                            load.status === "in-transit",
                        ).length
                      }
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
                    <p className="text-sm text-muted-foreground">
                      Delivered Today
                    </p>
                    <p className="text-2xl font-medium">
                      {
                        sampleLoads.filter(
                          (load) =>
                            load.status === "delivered" &&
                            load.actualDelivery?.startsWith(
                              new Date()
                                .toISOString()
                                .split("T")[0],
                            ),
                        ).length
                      }
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
                    <p className="text-sm text-muted-foreground">
                      Overdue
                    </p>
                    <p className="text-2xl font-medium">0</p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          {/* Loads List */}
          <div className="space-y-4">
            {filteredLoads.map((load) => (
              <Card
                key={load.id}
                className="hover:shadow-md transition-shadow"
              >
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <Truck className="h-5 w-5" />
                        {load.id}
                        <Badge
                          variant="outline"
                          className="capitalize"
                        >
                          {load.trailerType.replace("-", " ")}
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
                      <Badge
                        variant="outline"
                        className={getPriorityColor(
                          load.priority,
                        )}
                      >
                        {load.priority}
                      </Badge>
                      <Badge
                        variant="outline"
                        className={getStatusColor(load.status)}
                      >
                        {load.status.replace("-", " ")}
                      </Badge>
                      {load.deliveryRegion && (
                        <Badge
                          variant="outline"
                          className={
                            load.deliveryRegion === "local"
                              ? "bg-green-100 text-green-700 border-green-200"
                              : load.deliveryRegion ===
                                  "out-of-town"
                                ? "bg-blue-100 text-blue-700 border-blue-200"
                                : load.deliveryRegion ===
                                    "island-pool"
                                  ? "bg-purple-100 text-purple-700 border-purple-200"
                                  : load.deliveryRegion ===
                                      "okanagan-pool"
                                    ? "bg-orange-100 text-orange-700 border-orange-200"
                                    : "bg-gray-100 text-gray-700 border-gray-200"
                          }
                        >
                          {load.deliveryRegion.replace(
                            "-",
                            " ",
                          )}
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
                          {load.currentWeight.toLocaleString()}{" "}
                          / {load.maxWeight.toLocaleString()}{" "}
                          lbs
                        </span>
                      </div>
                      <Progress
                        value={load.utilizationPercentage}
                        className="h-2"
                      />
                      <div className="flex justify-between text-xs text-muted-foreground">
                        <span>
                          {load.utilizationPercentage}% utilized
                        </span>
                        <span>{load.items.length} stops</span>
                      </div>
                    </div>

                    {/* Schedule */}
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">
                          Scheduled Pickup:
                        </span>
                        <p className="font-medium flex items-center gap-1">
                          <Calendar className="h-3 w-3" />
                          {new Date(
                            load.scheduledPickup,
                          ).toLocaleString()}
                        </p>
                      </div>
                      {load.estimatedDelivery && (
                        <div>
                          <span className="text-muted-foreground">
                            Est. Delivery:
                          </span>
                          <p className="font-medium flex items-center gap-1">
                            <Clock className="h-3 w-3" />
                            {new Date(
                              load.estimatedDelivery,
                            ).toLocaleString()}
                          </p>
                        </div>
                      )}
                    </div>

                    {/* Route & Stops */}
                    <div className="space-y-2">
                      <span className="text-muted-foreground text-sm">
                        Route:
                      </span>
                      <div className="space-y-2">
                        {load.items.map((item, index) => (
                          <div
                            key={item.id}
                            className="flex items-start gap-3 p-3 bg-gray-50 rounded-lg"
                          >
                            <div className="flex-shrink-0 w-6 h-6 bg-teal-100 text-teal-700 rounded-full flex items-center justify-center text-xs font-medium">
                              {index + 1}
                            </div>
                            <div className="flex-1 min-w-0">
                              <div className="flex items-center justify-between">
                                <div>
                                  <p className="font-medium text-sm">
                                    {item.customerName}
                                  </p>
                                  <p className="text-xs text-muted-foreground">
                                    {item.orderNumber}
                                  </p>
                                </div>
                                <div className="text-right text-xs">
                                  <p className="font-medium">
                                    {item.weight.toLocaleString()}{" "}
                                    lbs
                                  </p>
                                  <p className="text-muted-foreground">
                                    {item.pieces} pcs
                                  </p>
                                </div>
                              </div>
                              <div className="mt-1">
                                <p className="text-xs text-muted-foreground flex items-center gap-1">
                                  <MapPin className="h-3 w-3" />
                                  {item.destination}
                                </p>
                                {item.specialInstructions && (
                                  <p className="text-xs text-orange-600 mt-1 bg-orange-50 p-1 rounded">
                                    ⚠️{" "}
                                    {item.specialInstructions}
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
                            <p className="text-sm font-medium text-blue-800">
                              Load Notes
                            </p>
                            <p className="text-sm text-blue-700">
                              {load.notes}
                            </p>
                          </div>
                        </div>
                      </div>
                    )}

                    {/* Actions */}
                    <div className="flex justify-between items-center pt-3 border-t">
                      <div className="text-xs text-muted-foreground">
                        Created by {load.createdBy} •{" "}
                        {new Date(
                          load.createdAt,
                        ).toLocaleDateString()}
                      </div>
                      <div className="flex gap-2">
                        <Button variant="outline" size="sm">
                          <Edit className="h-4 w-4 mr-2" />
                          Edit Load
                        </Button>
                        {load.status === "planning" && (
                          <Button size="sm">
                            <CheckCircle2 className="h-4 w-4 mr-2" />
                            Start Loading
                          </Button>
                        )}
                        {load.status === "loaded" && (
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

        <TabsContent
          value="simple-planning"
          className="space-y-4"
        >
          <LoadCreationSimplified />
        </TabsContent>

        <TabsContent value="tracking" className="space-y-4">
          <div className="space-y-4">
            {sampleLoads
              .filter((load) =>
                [
                  "dispatched",
                  "in-transit",
                  "delivered",
                ].includes(load.status),
              )
              .map((load) => (
                <Card key={load.id}>
                  <CardHeader>
                    <div className="flex items-center justify-between">
                      <CardTitle className="flex items-center gap-2">
                        <Navigation className="h-5 w-5" />
                        {load.id} - {load.truckNumber}
                      </CardTitle>
                      <Badge
                        variant="outline"
                        className={getStatusColor(load.status)}
                      >
                        {load.status.replace("-", " ")}
                      </Badge>
                    </div>
                  </CardHeader>
                  <CardContent>
                    <div className="space-y-4">
                      <div className="grid grid-cols-2 gap-4 text-sm">
                        <div>
                          <span className="text-muted-foreground">
                            Driver:
                          </span>
                          <p className="font-medium">
                            {load.driverName}
                          </p>
                        </div>
                        <div>
                          <span className="text-muted-foreground">
                            Phone:
                          </span>
                          <p className="font-medium">
                            {load.driverPhone}
                          </p>
                        </div>
                      </div>

                      {load.actualPickup && (
                        <div className="text-sm">
                          <span className="text-muted-foreground">
                            Actual Pickup:
                          </span>
                          <p className="font-medium">
                            {new Date(
                              load.actualPickup,
                            ).toLocaleString()}
                          </p>
                        </div>
                      )}

                      {load.actualDelivery && (
                        <div className="text-sm">
                          <span className="text-muted-foreground">
                            Delivered:
                          </span>
                          <p className="font-medium text-green-600">
                            {new Date(
                              load.actualDelivery,
                            ).toLocaleString()}
                          </p>
                        </div>
                      )}

                      <div className="space-y-2">
                        <span className="text-muted-foreground text-sm">
                          Delivery Progress:
                        </span>
                        <div className="space-y-2">
                          {load.items.map((item, index) => (
                            <div
                              key={item.id}
                              className="flex items-center gap-3"
                            >
                              <div
                                className={`w-4 h-4 rounded-full flex-shrink-0 ${
                                  load.status === "delivered"
                                    ? "bg-green-500"
                                    : load.status ===
                                        "in-transit"
                                      ? "bg-orange-500"
                                      : "bg-gray-300"
                                }`}
                              />
                              <div className="flex-1">
                                <p className="text-sm font-medium">
                                  {item.customerName}
                                </p>
                                <p className="text-xs text-muted-foreground">
                                  {item.destination}
                                </p>
                              </div>
                              {load.status === "delivered" && (
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
              <h2 className="text-xl font-semibold">
                Pool Truck Management
              </h2>
              <p className="text-muted-foreground">
                Manage consolidated deliveries for Island,
                Okanagan, and Out-of-Town routes
              </p>
            </div>
            <div className="flex gap-2">
              <Button variant="outline" size="sm">
                <Timer className="h-4 w-4 mr-2" />
                Schedule Pool
              </Button>
              <Button
                size="sm"
                className="bg-teal-600 hover:bg-teal-700"
              >
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
                    <p className="text-sm text-muted-foreground">
                      Island Pool
                    </p>
                    <p className="text-2xl font-medium">8</p>
                    <p className="text-xs text-muted-foreground">
                      orders waiting
                    </p>
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
                    <p className="text-sm text-muted-foreground">
                      Okanagan Pool
                    </p>
                    <p className="text-2xl font-medium">5</p>
                    <p className="text-xs text-muted-foreground">
                      orders waiting
                    </p>
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
                    <p className="text-sm text-muted-foreground">
                      Out-of-Town
                    </p>
                    <p className="text-2xl font-medium">3</p>
                    <p className="text-xs text-muted-foreground">
                      orders waiting
                    </p>
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
                    <p className="text-sm text-muted-foreground">
                      Pool Savings
                    </p>
                    <p className="text-2xl font-medium">
                      $1,240
                    </p>
                    <p className="text-xs text-muted-foreground">
                      this week
                    </p>
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
                  <Badge
                    variant="outline"
                    className="text-purple-600 border-purple-200"
                  >
                    Ferry Dependent
                  </Badge>
                  <span>Next departure: 2:00 PM</span>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-purple-50 border-purple-200">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">
                          VI-SOUTH-01
                        </p>
                        <p className="text-sm text-muted-foreground">
                          T-312 • Victoria Route
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className="bg-purple-100 text-purple-700"
                      >
                        Ready to Ship
                      </Badge>
                    </div>

                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">
                          78% (36,400 lbs)
                        </span>
                      </div>
                      <Progress value={78} className="h-2" />
                    </div>

                    <div className="space-y-2 mb-3">
                      <p className="text-sm font-medium">
                        Orders (2):
                      </p>
                      <div className="space-y-1">
                        <div className="flex justify-between text-xs">
                          <span>
                            VSW-5567 • Victoria Steel Works
                          </span>
                          <span>6,200 lbs</span>
                        </div>
                        <div className="flex justify-between text-xs">
                          <span>
                            NB-9987 • Nanaimo Builders
                          </span>
                          <span>5,200 lbs</span>
                        </div>
                      </div>
                    </div>

                    <div className="flex items-center gap-2 text-xs text-purple-600 mb-3">
                      <Navigation className="h-3 w-3" />
                      Ferry: Tsawwassen-Swartz Bay • $350 total
                    </div>

                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        className="flex-1"
                      >
                        <Plus className="h-4 w-4 mr-1" />
                        Add Order
                      </Button>
                      <Button
                        size="sm"
                        className="bg-purple-600 hover:bg-purple-700"
                      >
                        <CheckCircle2 className="h-4 w-4 mr-1" />
                        Dispatch
                      </Button>
                    </div>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">
                          VI-CENTRAL-01
                        </p>
                        <p className="text-sm text-muted-foreground">
                          T-789 • Nanaimo Route
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className="bg-yellow-100 text-yellow-700"
                      >
                        Consolidating
                      </Badge>
                    </div>

                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">
                          35% (16,100 lbs)
                        </span>
                      </div>
                      <Progress value={35} className="h-2" />
                    </div>

                    <div className="text-xs text-muted-foreground mb-3">
                      Consolidation deadline: Tomorrow 10:00 AM
                    </div>

                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        className="flex-1"
                      >
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
                  <Badge
                    variant="outline"
                    className="text-orange-600 border-orange-200"
                  >
                    Mountain Routes
                  </Badge>
                  <span>Weekly service</span>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-orange-50 border-orange-200">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">
                          OK-CENTRAL-01
                        </p>
                        <p className="text-sm text-muted-foreground">
                          T-098 • Kelowna Route
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className="bg-orange-100 text-orange-700"
                      >
                        Consolidating
                      </Badge>
                    </div>

                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">
                          62% (24,800 lbs)
                        </span>
                      </div>
                      <Progress value={62} className="h-2" />
                    </div>

                    <div className="space-y-2 mb-3">
                      <p className="text-sm font-medium">
                        Orders (1):
                      </p>
                      <div className="text-xs">
                        KCS-8854 • Kelowna Construction Supply •
                        8,900 lbs
                      </div>
                    </div>

                    <div className="text-xs text-muted-foreground mb-3">
                      Next departure: Friday 6:00 AM
                    </div>

                    <div className="flex gap-2">
                      <Button
                        size="sm"
                        variant="outline"
                        className="flex-1"
                      >
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
                        <p className="font-medium">
                          OK-NORTH-01
                        </p>
                        <p className="text-sm text-muted-foreground">
                          Available • Vernon/Kamloops
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className="bg-gray-100 text-gray-700"
                      >
                        Available
                      </Badge>
                    </div>

                    <div className="text-xs text-muted-foreground mb-3">
                      No orders assigned
                    </div>

                    <Button
                      size="sm"
                      variant="outline"
                      className="w-full"
                    >
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
                  <Badge
                    variant="outline"
                    className="text-blue-600 border-blue-200"
                  >
                    Multi-Lane
                  </Badge>
                  <span>2-3x weekly</span>
                </div>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="p-4 border rounded-lg bg-blue-50 border-blue-200">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">
                          AB-NORTH-01
                        </p>
                        <p className="text-sm text-muted-foreground">
                          T-401 • Calgary/Edmonton
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className="bg-blue-100 text-blue-700"
                      >
                        In Transit
                      </Badge>
                    </div>

                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>Utilization:</span>
                        <span className="font-medium">
                          85% (40,800 lbs)
                        </span>
                      </div>
                      <Progress value={85} className="h-2" />
                    </div>

                    <div className="space-y-2 mb-3">
                      <p className="text-sm font-medium">
                        Orders (1):
                      </p>
                      <div className="text-xs">
                        CSD-7789 • Calgary Steel Dynamics •
                        12,400 lbs
                      </div>
                    </div>

                    <div className="flex items-center gap-2 text-xs text-blue-600 mb-3">
                      <Truck className="h-3 w-3" />
                      ETA Calgary: Tomorrow 2:00 PM
                    </div>

                    <Button
                      size="sm"
                      variant="outline"
                      className="w-full"
                    >
                      <Navigation className="h-4 w-4 mr-1" />
                      Track Load
                    </Button>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <div className="flex items-center justify-between mb-3">
                      <div>
                        <p className="font-medium">
                          AB-SOUTH-01
                        </p>
                        <p className="text-sm text-muted-foreground">
                          T-432 • Medicine Hat/Lethbridge
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className="bg-gray-100 text-gray-700"
                      >
                        Available
                      </Badge>
                    </div>

                    <div className="text-xs text-muted-foreground mb-3">
                      Ready for next consolidation
                    </div>

                    <Button
                      size="sm"
                      variant="outline"
                      className="w-full"
                    >
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
                        <p className="font-medium text-green-800">
                          Island Pool Ready
                        </p>
                        <p className="text-sm text-green-700 mb-2">
                          VI-SOUTH-01 has optimal load and ferry
                          booking confirmed
                        </p>
                        <div className="space-y-1 text-xs text-green-600">
                          <p>• 78% utilization achieved</p>
                          <p>
                            • Ferry booking confirmed for 2:00
                            PM
                          </p>
                          <p>
                            • Saves $320 vs individual
                            deliveries
                          </p>
                        </div>
                        <Button
                          size="sm"
                          className="mt-3 bg-green-600 hover:bg-green-700"
                        >
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
                        <p className="font-medium text-yellow-800">
                          Okanagan Consolidation
                        </p>
                        <p className="text-sm text-yellow-700 mb-2">
                          Wait for 2 more orders to optimize
                          OK-CENTRAL-01
                        </p>
                        <div className="space-y-1 text-xs text-yellow-600">
                          <p>• Current: 62% utilization</p>
                          <p>
                            • Target: 85% with 2 more orders
                          </p>
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
                        <p className="font-medium">
                          Today's Pickups
                        </p>
                        <p className="text-sm text-muted-foreground">
                          9 orders scheduled
                        </p>
                      </div>
                      <Badge
                        variant="outline"
                        className="bg-blue-100 text-blue-700"
                      >
                        Active
                      </Badge>
                    </div>

                    <div className="space-y-2 mb-3">
                      <div className="flex justify-between text-sm">
                        <span>
                          PFW-4412 • Pacific Fabrication
                        </span>
                        <span>10:00 AM</span>
                      </div>
                      <div className="flex justify-between text-sm">
                        <span>2 more scheduled</span>
                        <span>This afternoon</span>
                      </div>
                    </div>

                    <Button
                      size="sm"
                      variant="outline"
                      className="w-full"
                    >
                      <Calendar className="h-4 w-4 mr-1" />
                      Manage Schedule
                    </Button>
                  </div>

                  <div className="p-4 border rounded-lg">
                    <p className="font-medium mb-2">
                      Pickup Metrics
                    </p>
                    <div className="space-y-2 text-sm">
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">
                          Orders ready:
                        </span>
                        <span>15</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">
                          Avg wait time:
                        </span>
                        <span>12 minutes</span>
                      </div>
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">
                          Cost savings:
                        </span>
                        <span className="text-green-600">
                          $0/delivery
                        </span>
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
              <h2 className="text-xl font-semibold">
                Shipping Execution
              </h2>
              <p className="text-muted-foreground">
                Manage active shipments and dispatch operations
              </p>
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
                    <p className="text-sm text-muted-foreground">
                      Ready to Load
                    </p>
                    <p className="text-2xl font-medium">
                      {
                        sampleLoads.filter(
                          (load) => load.status === "planning",
                        ).length
                      }
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
                    <p className="text-sm text-muted-foreground">
                      Loading
                    </p>
                    <p className="text-2xl font-medium">
                      {
                        sampleLoads.filter(
                          (load) => load.status === "loading",
                        ).length
                      }
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
                    <p className="text-sm text-muted-foreground">
                      Ready to Dispatch
                    </p>
                    <p className="text-2xl font-medium">
                      {
                        sampleLoads.filter(
                          (load) => load.status === "loaded",
                        ).length
                      }
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
                    <p className="text-sm text-muted-foreground">
                      In Transit
                    </p>
                    <p className="text-2xl font-medium">
                      {
                        sampleLoads.filter(
                          (load) =>
                            load.status === "in-transit",
                        ).length
                      }
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
                  <p className="font-medium text-blue-800">
                    Partial Load Optimization Available
                  </p>
                  <p className="text-sm text-blue-600">
                    3 partial loads can be consolidated for
                    better efficiency.
                    <Button
                      variant="link"
                      className="h-auto p-0 text-blue-600"
                    >
                      View recommendations →
                    </Button>
                  </p>
                </div>
                <Button
                  variant="outline"
                  size="sm"
                  className="text-blue-600 border-blue-300"
                >
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
                    .filter((load) =>
                      [
                        "planning",
                        "loading",
                        "loaded",
                      ].includes(load.status),
                    )
                    .map((load) => (
                      <div
                        key={load.id}
                        className="p-4 border rounded-lg"
                      >
                        <div className="flex items-center justify-between mb-3">
                          <div>
                            <p className="font-medium">
                              {load.id}
                            </p>
                            <p className="text-sm text-muted-foreground">
                              {load.driverName} •{" "}
                              {load.truckNumber}
                            </p>
                          </div>
                          <div className="flex items-center gap-2">
                            <Badge
                              variant="outline"
                              className={getStatusColor(
                                load.status,
                              )}
                            >
                              {load.status.replace("-", " ")}
                            </Badge>
                            <Badge
                              variant="outline"
                              className={getPriorityColor(
                                load.priority,
                              )}
                            >
                              {load.priority}
                            </Badge>
                          </div>
                        </div>

                        <div className="grid grid-cols-2 gap-4 text-sm mb-3">
                          <div>
                            <span className="text-muted-foreground">
                              Scheduled Pickup:
                            </span>
                            <p className="font-medium">
                              {new Date(
                                load.scheduledPickup,
                              ).toLocaleDateString()}
                            </p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">
                              Stops:
                            </span>
                            <p className="font-medium">
                              {load.items.length}
                            </p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">
                              Weight:
                            </span>
                            <p className="font-medium">
                              {load.currentWeight.toLocaleString()}{" "}
                              lbs
                            </p>
                          </div>
                          <div>
                            <span className="text-muted-foreground">
                              Utilization:
                            </span>
                            <p className="font-medium">
                              {load.utilizationPercentage}%
                            </p>
                          </div>
                        </div>

                        <div className="flex justify-end gap-2">
                          {load.status === "planning" && (
                            <Button size="sm" variant="outline">
                              <Package className="h-4 w-4 mr-2" />
                              Start Loading
                            </Button>
                          )}
                          {load.status === "loading" && (
                            <Button size="sm" variant="outline">
                              <CheckCircle2 className="h-4 w-4 mr-2" />
                              Mark Loaded
                            </Button>
                          )}
                          {load.status === "loaded" && (
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
                    .filter((load) =>
                      ["dispatched", "in-transit"].includes(
                        load.status,
                      ),
                    )
                    .map((load) => (
                      <div
                        key={load.id}
                        className="p-4 border rounded-lg"
                      >
                        <div className="flex items-center justify-between mb-2">
                          <div>
                            <p className="font-medium">
                              {load.driverName}
                            </p>
                            <p className="text-sm text-muted-foreground">
                              {load.id}
                            </p>
                          </div>
                          <Badge
                            variant="outline"
                            className="bg-green-100 text-green-800"
                          >
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
              <h2 className="text-xl font-semibold">
                Live Tracking
              </h2>
              <p className="text-muted-foreground">
                Real-time location and status tracking for
                active shipments
              </p>
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
                      .filter((load) =>
                        ["dispatched", "in-transit"].includes(
                          load.status,
                        ),
                      )
                      .map((load) => (
                        <div
                          key={load.id}
                          className="p-4 border rounded-lg"
                        >
                          <div className="flex items-center justify-between mb-3">
                            <div>
                              <p className="font-medium">
                                {load.id}
                              </p>
                              <p className="text-sm text-muted-foreground">
                                {load.driverName} •{" "}
                                {load.truckNumber}
                              </p>
                            </div>
                            <div className="flex items-center gap-2">
                              <Badge
                                variant="outline"
                                className="bg-green-100 text-green-700"
                              >
                                Live
                              </Badge>
                              <Badge
                                variant="outline"
                                className={getStatusColor(
                                  load.status,
                                )}
                              >
                                {load.status.replace("-", " ")}
                              </Badge>
                            </div>
                          </div>

                          {load.currentLocation && (
                            <div className="mb-3 p-3 bg-blue-50 rounded-lg">
                              <div className="flex items-start gap-2">
                                <MapPin className="h-4 w-4 text-blue-600 mt-0.5" />
                                <div className="flex-1">
                                  <p className="text-sm font-medium text-blue-800">
                                    Current Location
                                  </p>
                                  <p className="text-sm text-blue-700">
                                    {
                                      load.currentLocation
                                        .address
                                    }
                                  </p>
                                  <p className="text-xs text-blue-600">
                                    Last updated:{" "}
                                    {new Date(
                                      load.currentLocation.timestamp,
                                    ).toLocaleString()}
                                  </p>
                                </div>
                              </div>
                            </div>
                          )}

                          <div className="mb-3">
                            <p className="text-sm font-medium mb-2">
                              Route Progress:
                            </p>
                            <div className="space-y-2">
                              {load.items.map((item, index) => (
                                <div
                                  key={item.id}
                                  className="flex items-center gap-3"
                                >
                                  <div
                                    className={`flex-shrink-0 w-6 h-6 rounded-full flex items-center justify-center text-xs font-medium ${
                                      load.status ===
                                        "in-transit" &&
                                      index === 0
                                        ? "bg-orange-500 text-white"
                                        : "bg-gray-300 text-gray-600"
                                    }`}
                                  >
                                    {index + 1}
                                  </div>
                                  <div className="flex-1 min-w-0">
                                    <p className="font-medium text-sm">
                                      {item.customerName}
                                    </p>
                                    <p className="text-xs text-muted-foreground">
                                      {item.destination}
                                    </p>
                                  </div>
                                  {load.status ===
                                    "delivered" && (
                                    <CheckCircle2 className="h-4 w-4 text-green-500" />
                                  )}
                                </div>
                              ))}
                            </div>
                          </div>

                          <div className="flex justify-between items-center">
                            <div className="text-xs text-muted-foreground">
                              Est. delivery:{" "}
                              {load.estimatedDelivery
                                ? new Date(
                                    load.estimatedDelivery,
                                  ).toLocaleString()
                                : "TBD"}
                            </div>
                            <div className="flex gap-2">
                              <Button
                                size="sm"
                                variant="outline"
                              >
                                <Phone className="h-4 w-4 mr-1" />
                                Call
                              </Button>
                              <Button
                                size="sm"
                                variant="outline"
                              >
                                <MessageSquare className="h-4 w-4 mr-1" />
                                Message
                              </Button>
                              <Button
                                size="sm"
                                variant="outline"
                              >
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
                      <span className="text-sm text-muted-foreground">
                        Active Shipments:
                      </span>
                      <span className="font-medium">
                        {
                          sampleLoads.filter((load) =>
                            [
                              "dispatched",
                              "in-transit",
                            ].includes(load.status),
                          ).length
                        }
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">
                        On Schedule:
                      </span>
                      <span className="font-medium text-green-600">
                        {
                          sampleLoads.filter(
                            (load) =>
                              load.status === "in-transit",
                          ).length
                        }
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">
                        Delayed:
                      </span>
                      <span className="font-medium text-red-600">
                        0
                      </span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-sm text-muted-foreground">
                        Exceptions:
                      </span>
                      <span className="font-medium text-orange-600">
                        0
                      </span>
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
                          <p className="text-sm font-medium text-green-800">
                            All Systems Normal
                          </p>
                          <p className="text-xs text-green-600">
                            All shipments tracking properly
                          </p>
                        </div>
                      </div>
                    </div>

                    <div className="p-3 bg-blue-50 border border-blue-200 rounded-lg">
                      <div className="flex items-center gap-2">
                        <Info className="h-4 w-4 text-blue-600" />
                        <div>
                          <p className="text-sm font-medium text-blue-800">
                            Ferry Schedule Update
                          </p>
                          <p className="text-xs text-blue-600">
                            Island routes updated for weather
                          </p>
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
              <h2 className="text-xl font-semibold">
                Shipping Analytics
              </h2>
              <p className="text-muted-foreground">
                Performance metrics and insights
              </p>
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
                    <p className="text-sm text-muted-foreground">
                      On-Time Delivery
                    </p>
                    <p className="text-2xl font-medium">
                      95.2%
                    </p>
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
                    <p className="text-sm text-muted-foreground">
                      Avg Load Utilization
                    </p>
                    <p className="text-2xl font-medium">
                      78.5%
                    </p>
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
                    <p className="text-sm text-muted-foreground">
                      Cost Per Mile
                    </p>
                    <p className="text-2xl font-medium">
                      $2.85
                    </p>
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
                    <p className="text-sm text-muted-foreground">
                      Fuel Efficiency
                    </p>
                    <p className="text-2xl font-medium">
                      6.8 MPG
                    </p>
                  </div>
                </div>
              </CardContent>
            </Card>
          </div>

          <div className="grid grid-cols-1 lg:grid-cols-2 gap-6">
            {/* Performance Trends */}
            <Card>
              <CardHeader>
                <CardTitle>
                  Delivery Performance Trends
                </CardTitle>
              </CardHeader>
              <CardContent>
                <div className="space-y-4">
                  <div className="grid grid-cols-3 gap-4 text-center">
                    <div className="p-3 bg-green-50 rounded-lg">
                      <p className="text-sm text-muted-foreground">
                        This Week
                      </p>
                      <p className="text-lg font-medium text-green-600">
                        96.8%
                      </p>
                    </div>
                    <div className="p-3 bg-blue-50 rounded-lg">
                      <p className="text-sm text-muted-foreground">
                        This Month
                      </p>
                      <p className="text-lg font-medium text-blue-600">
                        94.2%
                      </p>
                    </div>
                    <div className="p-3 bg-purple-50 rounded-lg">
                      <p className="text-sm text-muted-foreground">
                        This Quarter
                      </p>
                      <p className="text-lg font-medium text-purple-600">
                        95.1%
                      </p>
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
                  {availableTrucks.map((truck) => (
                    <div
                      key={truck.id}
                      className="flex items-center justify-between"
                    >
                      <div>
                        <p className="font-medium">
                          {truck.number}
                        </p>
                        <p className="text-sm text-muted-foreground capitalize">
                          {truck.type.replace("-", " ")}
                        </p>
                      </div>
                      <div className="text-right">
                        <p className="text-sm font-medium">
                          {truck.status === "assigned"
                            ? "85%"
                            : truck.status === "available"
                              ? "0%"
                              : "N/A"}
                        </p>
                        <Badge
                          variant={
                            truck.status === "assigned"
                              ? "default"
                              : truck.status === "available"
                                ? "secondary"
                                : "destructive"
                          }
                        >
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
        <Dialog
          open={showLoadBuilder}
          onOpenChange={setShowLoadBuilder}
        >
          <DialogContent className="max-w-5xl">
            <DialogHeader>
              <DialogTitle>Create New Load</DialogTitle>
              <DialogDescription>
                Configure truck, driver, and scheduling details
                for the new load. Select orders to include.
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
                      onChange={() =>
                        setNewLoad({
                          ...newLoad,
                          isPartialLoad: false,
                          maxUtilization: 100,
                        })
                      }
                      className="text-teal-600"
                    />
                    <label
                      htmlFor="fullLoad"
                      className="text-sm font-medium"
                    >
                      Full Load
                    </label>
                  </div>
                  <div className="flex items-center gap-2">
                    <input
                      type="radio"
                      id="partialLoad"
                      name="loadType"
                      checked={newLoad.isPartialLoad}
                      onChange={() =>
                        setNewLoad({
                          ...newLoad,
                          isPartialLoad: true,
                          maxUtilization: 70,
                        })
                      }
                      className="text-teal-600"
                    />
                    <label
                      htmlFor="partialLoad"
                      className="text-sm font-medium"
                    >
                      Partial Load
                    </label>
                  </div>
                  {newLoad.isPartialLoad && (
                    <div className="ml-4 flex items-center gap-2">
                      <label className="text-sm">
                        Max Utilization:
                      </label>
                      <Input
                        type="number"
                        min="20"
                        max="100"
                        value={newLoad.maxUtilization}
                        onChange={(e) =>
                          setNewLoad({
                            ...newLoad,
                            maxUtilization: parseInt(
                              e.target.value,
                            ),
                          })
                        }
                        className="w-20"
                      />
                      <span className="text-sm">%</span>
                    </div>
                  )}
                </div>
                {newLoad.isPartialLoad && (
                  <p className="text-sm text-muted-foreground mt-2">
                    Partial loads allow for consolidation with
                    other orders and faster dispatch times.
                  </p>
                )}
              </div>

              <div className="grid grid-cols-2 gap-6">
                {/* Load Configuration */}
                <div className="space-y-4">
                  <h3 className="font-medium">
                    Load Configuration
                  </h3>
                  <div className="grid grid-cols-1 gap-4">
                    <div>
                      <label className="text-sm font-medium">
                        Truck
                      </label>
                      <Select
                        value={newLoad.truckNumber}
                        onValueChange={(value) =>
                          setNewLoad({
                            ...newLoad,
                            truckNumber: value,
                          })
                        }
                      >
                        <SelectTrigger>
                          <SelectValue placeholder="Select truck..." />
                        </SelectTrigger>
                        <SelectContent>
                          {availableTrucks
                            .filter(
                              (truck) =>
                                truck.status === "available",
                            )
                            .map((truck) => (
                              <SelectItem
                                key={truck.id}
                                value={truck.number}
                              >
                                {truck.number} -{" "}
                                {truck.type.replace("-", " ")} (
                                {truck.maxWeight.toLocaleString()}{" "}
                                lbs)
                              </SelectItem>
                            ))}
                        </SelectContent>
                      </Select>
                    </div>
                    <div>
                      <label className="text-sm font-medium">
                        Driver
                      </label>
                      <Input
                        value={newLoad.driverName}
                        onChange={(e) =>
                          setNewLoad({
                            ...newLoad,
                            driverName: e.target.value,
                          })
                        }
                        placeholder="Driver name..."
                      />
                    </div>
                    <div>
                      <label className="text-sm font-medium">
                        Driver Phone
                      </label>
                      <Input
                        value={newLoad.driverPhone}
                        onChange={(e) =>
                          setNewLoad({
                            ...newLoad,
                            driverPhone: e.target.value,
                          })
                        }
                        placeholder="(555) 123-4567"
                      />
                    </div>
                    <div>
                      <label className="text-sm font-medium">
                        Scheduled Pickup
                      </label>
                      <Input
                        type="datetime-local"
                        value={newLoad.scheduledPickup}
                        onChange={(e) =>
                          setNewLoad({
                            ...newLoad,
                            scheduledPickup: e.target.value,
                          })
                        }
                      />
                    </div>
                  </div>

                  <div>
                    <label className="text-sm font-medium">
                      Load Notes
                    </label>
                    <Textarea
                      value={newLoad.notes}
                      onChange={(e) =>
                        setNewLoad({
                          ...newLoad,
                          notes: e.target.value,
                        })
                      }
                      placeholder="Special instructions or notes..."
                      rows={3}
                    />
                  </div>
                </div>

                {/* Selected Orders */}
                <div className="space-y-4">
                  <div className="flex items-center justify-between">
                    <h3 className="font-medium">
                      Selected Orders ({selectedOrders.length})
                    </h3>
                    <Button
                      variant="outline"
                      size="sm"
                      onClick={() => setSelectedOrders([])}
                    >
                      Clear All
                    </Button>
                  </div>

                  <div className="border rounded-lg p-4 bg-gray-50 max-h-64 overflow-y-auto">
                    {selectedOrders.length === 0 ? (
                      <p className="text-center text-muted-foreground py-8">
                        No orders selected. Go back to Load
                        Planning to select orders.
                      </p>
                    ) : (
                      <div className="space-y-3">
                        {availableOrders
                          .filter((order) =>
                            selectedOrders.includes(order.id),
                          )
                          .map((order) => (
                            <div
                              key={order.id}
                              className="flex items-center justify-between p-3 bg-white rounded border"
                            >
                              <div>
                                <p className="font-medium text-sm">
                                  {order.customerName}
                                </p>
                                <p className="text-xs text-muted-foreground">
                                  {order.orderNumber}
                                </p>
                                <p className="text-xs text-muted-foreground">
                                  {order.city}, {order.state}
                                </p>
                              </div>
                              <div className="text-right">
                                <p className="text-sm font-medium">
                                  {order.weight.toLocaleString()}{" "}
                                  lbs
                                </p>
                                <p className="text-xs text-muted-foreground">
                                  {order.pieces} pcs
                                </p>
                                <Button
                                  size="sm"
                                  variant="ghost"
                                  onClick={() =>
                                    setSelectedOrders(
                                      selectedOrders.filter(
                                        (id) => id !== order.id,
                                      ),
                                    )
                                  }
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
                      <h4 className="font-medium mb-3">
                        Load Summary
                      </h4>
                      <div className="space-y-2 text-sm">
                        <div className="flex justify-between">
                          <span>Total Weight:</span>
                          <span className="font-medium">
                            {availableOrders
                              .filter((order) =>
                                selectedOrders.includes(
                                  order.id,
                                ),
                              )
                              .reduce(
                                (sum, order) =>
                                  sum + order.weight,
                                0,
                              )
                              .toLocaleString()}{" "}
                            lbs
                          </span>
                        </div>

                        <div className="flex justify-between">
                          <span>Stops:</span>
                          <span className="font-medium">
                            {selectedOrders.length}
                          </span>
                        </div>
                        {newLoad.truckNumber && (
                          <div className="flex justify-between">
                            <span>Utilization:</span>
                            <span className="font-medium">
                              {Math.round(
                                (availableOrders
                                  .filter((order) =>
                                    selectedOrders.includes(
                                      order.id,
                                    ),
                                  )
                                  .reduce(
                                    (sum, order) =>
                                      sum + order.weight,
                                    0,
                                  ) /
                                  (availableTrucks.find(
                                    (t) =>
                                      t.number ===
                                      newLoad.truckNumber,
                                  )?.maxWeight || 1)) *
                                  100,
                              )}
                              %
                            </span>
                          </div>
                        )}
                      </div>

                      {newLoad.isPartialLoad && (
                        <div className="mt-3 p-2 bg-blue-50 rounded text-xs">
                          <p className="text-blue-800">
                            ⓘ This partial load can be
                            consolidated with other orders later
                            or dispatched early for
                            time-sensitive deliveries.
                          </p>
                        </div>
                      )}
                    </div>
                  )}
                </div>
              </div>

              <div className="flex justify-end gap-2">
                <Button
                  variant="outline"
                  onClick={() => setShowLoadBuilder(false)}
                >
                  Cancel
                </Button>
                <Button
                  className="bg-teal-600 hover:bg-teal-700"
                  disabled={
                    selectedOrders.length === 0 ||
                    !newLoad.truckNumber ||
                    !newLoad.driverName
                  }
                >
                  <CheckCircle2 className="h-4 w-4 mr-2" />
                  {newLoad.isPartialLoad
                    ? "Create Partial Load"
                    : "Create Full Load"}
                </Button>
              </div>
            </div>
          </DialogContent>
        </Dialog>

        {/* Dispatch Dialog */}
        <Dialog
          open={showDispatchDialog}
          onOpenChange={setShowDispatchDialog}
        >
          <DialogContent className="max-w-2xl">
            <DialogHeader>
              <DialogTitle>
                Dispatch Load - {selectedLoad?.id}
              </DialogTitle>
              <DialogDescription>
                Complete the pre-dispatch checklist and dispatch
                the load to the driver.
              </DialogDescription>
            </DialogHeader>
            {selectedLoad && (
              <div className="space-y-6">
                <div className="p-4 bg-gray-50 rounded-lg">
                  <div className="grid grid-cols-2 gap-4 text-sm">
                    <div>
                      <span className="text-muted-foreground">
                        Driver:
                      </span>
                      <p className="font-medium">
                        {selectedLoad.driverName}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">
                        Truck:
                      </span>
                      <p className="font-medium">
                        {selectedLoad.truckNumber}
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">
                        Total Weight:
                      </span>
                      <p className="font-medium">
                        {selectedLoad.currentWeight.toLocaleString()}{" "}
                        lbs
                      </p>
                    </div>
                    <div>
                      <span className="text-muted-foreground">
                        Stops:
                      </span>
                      <p className="font-medium">
                        {selectedLoad.items.length}
                      </p>
                    </div>
                  </div>
                </div>

                <div className="space-y-3">
                  <h3 className="font-medium">
                    Pre-Dispatch Checklist
                  </h3>
                  {[
                    "Load inspection completed",
                    "Load securement verified",
                    "Driver route briefing completed",
                    "Customer delivery windows confirmed",
                    "Emergency contact information provided",
                    "Vehicle inspection completed",
                  ].map((item, index) => (
                    <div
                      key={index}
                      className="flex items-center gap-3"
                    >
                      <Checkbox />
                      <span className="text-sm">{item}</span>
                    </div>
                  ))}
                </div>

                <div>
                  <label className="text-sm font-medium">
                    Dispatch Notes
                  </label>
                  <Textarea
                    placeholder="Any special instructions for the driver..."
                    rows={3}
                  />
                </div>

                <div className="flex justify-end gap-2">
                  <Button
                    variant="outline"
                    onClick={() => setShowDispatchDialog(false)}
                  >
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