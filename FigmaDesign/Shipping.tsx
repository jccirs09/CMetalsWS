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
}

const truckTypes = ["flatbed", "enclosed", "step-deck", "dry-van", "refrigerated"];

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
    fuelEfficiency: 6.5
  },
  {
    id: "T-205",
    number: "T-205", 
    type: "enclosed",
    maxWeight: 44000,
    maxDimensions: { length: 45, width: 8, height: 9 },
    status: "available",
    location: "Yard",
    fuelEfficiency: 7.2
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
    fuelEfficiency: 6.0
  },
  {
    id: "T-156",
    number: "T-156",
    type: "dry-van",
    maxWeight: 42000,
    maxDimensions: { length: 40, width: 8, height: 9 },
    status: "available",
    location: "Maintenance Bay",
    fuelEfficiency: 7.8
  },
  {
    id: "T-098",
    number: "T-098",
    type: "refrigerated",
    maxWeight: 40000,
    maxDimensions: { length: 40, width: 8, height: 9 },
    status: "available",
    location: "Yard",
    fuelEfficiency: 6.8
  }
];

const availableOrders: AvailableOrder[] = [
  {
    id: "ORD-2024-0094",
    pickingListId: "PL-2024-0094",
    customerName: "Midwest Manufacturing",
    orderNumber: "MM-5567",
    destination: "2340 Industrial Pkwy, Cedar Rapids, IA 52404",
    city: "Cedar Rapids",
    state: "IA", 
    zipCode: "52404",
    weight: 6200,
    pieces: 35,
    dimensions: { length: 10, width: 4, height: 2 },
    priority: "high",
    requiredDeliveryDate: "2024-01-19",
    readyDate: "2024-01-18",
    customerContact: "Jim Peterson",
    customerPhone: "(319) 555-0178",
    specialInstructions: "Dock height delivery required",
    loadingRequirements: "Forklift required",
    unloadingRequirements: "Customer has crane",
    pickingStatus: "ready",
    value: 28500,
    distance: 185
  },
  {
    id: "ORD-2024-0095",
    pickingListId: "PL-2024-0095",
    customerName: "Steel Dynamics Inc",
    orderNumber: "SDI-7789",
    destination: "890 Steel Mill Rd, Gary, IN 46402",
    city: "Gary",
    state: "IN",
    zipCode: "46402", 
    weight: 12400,
    pieces: 20,
    dimensions: { length: 20, width: 8, height: 3 },
    priority: "urgent",
    requiredDeliveryDate: "2024-01-18",
    readyDate: "2024-01-17",
    customerContact: "Maria Gonzalez",
    customerPhone: "(219) 555-0234",
    specialInstructions: "Security clearance required at gate",
    loadingRequirements: "Overhead crane needed",
    unloadingRequirements: "Customer unloads",
    pickingStatus: "packed",
    value: 45200,
    distance: 125
  },
  {
    id: "ORD-2024-0096", 
    pickingListId: "PL-2024-0096",
    customerName: "Automotive Solutions",
    orderNumber: "AS-3321",
    destination: "1500 Auto Parts Way, Detroit, MI 48201",
    city: "Detroit",
    state: "MI",
    zipCode: "48201",
    weight: 3850,
    pieces: 75,
    dimensions: { length: 6, width: 3, height: 1.5 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-20",
    readyDate: "2024-01-18", 
    customerContact: "Robert Kim",
    customerPhone: "(313) 555-0445",
    specialInstructions: "Morning delivery preferred",
    loadingRequirements: "Standard loading",
    unloadingRequirements: "Customer forklift available",
    pickingStatus: "ready",
    value: 18750,
    distance: 290
  },
  {
    id: "ORD-2024-0097",
    pickingListId: "PL-2024-0097", 
    customerName: "Construction Supply Co",
    orderNumber: "CSC-8854",
    destination: "780 Builder Ave, Madison, WI 53703",
    city: "Madison",
    state: "WI",
    zipCode: "53703",
    weight: 8900,
    pieces: 45,
    dimensions: { length: 12, width: 6, height: 2.5 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-21",
    readyDate: "2024-01-19",
    customerContact: "Lisa Chen",
    customerPhone: "(608) 555-0667",
    specialInstructions: "Call 30 minutes before arrival",
    loadingRequirements: "Side loading preferred",
    unloadingRequirements: "Dock level unloading",
    pickingStatus: "picked",
    value: 32400,
    distance: 220
  },
  {
    id: "ORD-2024-0098",
    pickingListId: "PL-2024-0098",
    customerName: "Fabrication Works",
    orderNumber: "FW-4412",
    destination: "950 Fab Shop Rd, Rockford, IL 61101",
    city: "Rockford", 
    state: "IL",
    zipCode: "61101",
    weight: 15600,
    pieces: 12,
    dimensions: { length: 25, width: 8, height: 4 },
    priority: "high",
    requiredDeliveryDate: "2024-01-19",
    readyDate: "2024-01-18",
    customerContact: "David Johnson",
    customerPhone: "(815) 555-0889",
    specialInstructions: "Heavy lift equipment needed",
    loadingRequirements: "Overhead crane required",
    unloadingRequirements: "Customer crane available",
    pickingStatus: "ready",
    value: 62300,
    distance: 95
  },
  {
    id: "ORD-2024-0099",
    pickingListId: "PL-2024-0099",
    customerName: "Metro Builders",
    orderNumber: "MB-9987",
    destination: "450 Metro Center Dr, Milwaukee, WI 53202",
    city: "Milwaukee",
    state: "WI", 
    zipCode: "53202",
    weight: 5200,
    pieces: 60,
    dimensions: { length: 8, width: 4, height: 1 },
    priority: "normal",
    requiredDeliveryDate: "2024-01-22",
    readyDate: "2024-01-19",
    customerContact: "Amanda Foster",
    customerPhone: "(414) 555-0334",
    specialInstructions: "Inside delivery required",
    loadingRequirements: "Hand truck needed",
    unloadingRequirements: "Ground level delivery",
    pickingStatus: "ready",
    value: 21800,
    distance: 165
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
      lat: 41.8781,
      lng: -87.6298,
      address: "I-90 E near Chicago, IL",
      timestamp: "2024-01-18T11:30:00Z"
    },
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
  const [activeTab, setActiveTab] = useState("loads");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [selectedTrailerType, setSelectedTrailerType] = useState("all");
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
    notes: ""
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
        <TabsList className="grid w-full grid-cols-5">
          <TabsTrigger value="loads" className="flex items-center gap-2">
            <Truck className="h-4 w-4" />
            Load Management
          </TabsTrigger>
          <TabsTrigger value="planning" className="flex items-center gap-2">
            <Calculator className="h-4 w-4" />
            Load Planning
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
          </div>

          <Button>
            <Plus className="h-4 w-4 mr-2" />
            New Load
          </Button>
        </div>

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

                        <Button 
                          className="w-full" 
                          disabled={selectedOrders.length === 0}
                          onClick={() => setShowLoadBuilder(true)}
                        >
                          <Truck className="h-4 w-4 mr-2" />
                          Create Load
                        </Button>
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
          <DialogContent className="max-w-4xl">
            <DialogHeader>
              <DialogTitle>Create New Load</DialogTitle>
              <DialogDescription>
                Configure truck, driver, and scheduling details for the new load.
              </DialogDescription>
            </DialogHeader>
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4">
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

              <div className="flex justify-end gap-2">
                <Button variant="outline" onClick={() => setShowLoadBuilder(false)}>
                  Cancel
                </Button>
                <Button className="bg-teal-600 hover:bg-teal-700">
                  <CheckCircle2 className="h-4 w-4 mr-2" />
                  Create Load
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