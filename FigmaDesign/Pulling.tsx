import { useState } from "react";
import { Card, CardContent, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Select, SelectContent, SelectItem, SelectTrigger, SelectValue } from "./ui/select";
import { Checkbox } from "./ui/checkbox";
import { Tabs, TabsContent, TabsList, TabsTrigger } from "./ui/tabs";
import { Dialog, DialogContent, DialogHeader, DialogTitle, DialogTrigger } from "./ui/dialog";
import { Textarea } from "./ui/textarea";
import { Progress } from "./ui/progress";
import { 
  Package, 
  MapPin, 
  Clock, 
  Search, 
  Filter, 
  CheckCircle2, 
  AlertCircle,
  Truck,
  Scan,
  RotateCcw,
  User,
  Box,
  Clipboard,
  Weight,
  Ruler,
  Eye,
  Play,
  Pause,
  Save,
  PrinterIcon,
  QrCode,
  Shield
} from "lucide-react";
import { StatusChip } from "./StatusChip";

interface LineItem {
  id: string;
  material: string;
  gauge: string;
  width: number;
  length: number;
  weight: number;
  color?: string;
  quantity: number;
  location: string;
  coilId?: string;
  picked: boolean;
  pickedBy?: string;
  pickedAt?: string;
  packed: boolean;
  packedBy?: string;
  packedAt?: string;
  packingMaterial?: string;
  packingNotes?: string;
  qualityChecked?: boolean;
  qualityCheckedBy?: string;
  qualityCheckedAt?: string;
  actualWeight?: number;
  damageNotes?: string;
}

interface PickingList {
  id: string;
  workOrderId: string;
  customerName: string;
  orderNumber: string;
  destination: string;
  priority: "low" | "normal" | "high" | "urgent";
  status: "planned" | "ready" | "picking" | "picked" | "packed" | "shipped";
  dueDate: string;
  assignedTo?: string;
  lineItems: LineItem[];
  createdAt: string;
  completedAt?: string;
  totalWeight: number;
  packingNotes?: string;
}

const destinations = [
  "Main Warehouse",
  "Dock A - Local Delivery",
  "Dock B - LTL Shipping", 
  "Dock C - Freight",
  "Quality Hold",
  "Customer Pickup"
];

const packingMaterials = [
  "Protective Wrap",
  "Wooden Crating",
  "Steel Strapping", 
  "Cardboard Protection",
  "Foam Padding",
  "Plastic Sheeting",
  "Moisture Barrier",
  "Custom Packaging"
];

const samplePickingLists: PickingList[] = [
  {
    id: "PL-2024-0089",
    workOrderId: "WO-2024-0156",
    customerName: "Precision Manufacturing",
    orderNumber: "PM-8891",
    destination: "Dock A - Local Delivery",
    priority: "high",
    status: "picking",
    dueDate: "2024-01-18",
    assignedTo: "John Miller",
    totalWeight: 5700,
    createdAt: "2024-01-15T08:30:00Z",
    lineItems: [
      {
        id: "LI-001",
        material: "Cold Rolled Steel",
        gauge: "16ga",
        width: 48,
        length: 96,
        weight: 2850,
        color: "Galvanized",
        quantity: 25,
        location: "A-12-C",
        coilId: "CRS-48-16-001",
        picked: true,
        pickedBy: "John Miller",
        pickedAt: "2024-01-15T09:15:00Z",
        packed: false,
        qualityChecked: true,
        qualityCheckedBy: "John Miller",
        qualityCheckedAt: "2024-01-15T09:20:00Z",
        actualWeight: 2850
      },
      {
        id: "LI-002",
        material: "Cold Rolled Steel",
        gauge: "16ga", 
        width: 48,
        length: 120,
        weight: 2850,
        color: "Galvanized",
        quantity: 25,
        location: "A-12-D",
        coilId: "CRS-48-16-002",
        picked: false,
        packed: false,
        qualityChecked: false
      }
    ]
  },
  {
    id: "PL-2024-0090",
    workOrderId: "WO-2024-0157",
    customerName: "ABC Construction",
    orderNumber: "ABC-4412",
    destination: "Dock B - LTL Shipping",
    priority: "normal",
    status: "ready",
    dueDate: "2024-01-19",
    totalWeight: 3720,
    createdAt: "2024-01-15T10:00:00Z",
    lineItems: [
      {
        id: "LI-003",
        material: "Aluminum Sheet",
        gauge: "0.125",
        width: 60,
        length: 120,
        weight: 1240,
        quantity: 50,
        location: "B-08-A",
        coilId: "AL-60-125-003",
        picked: false,
        packed: false,
        qualityChecked: false
      },
      {
        id: "LI-004",
        material: "Aluminum Sheet",
        gauge: "0.125",
        width: 48,
        length: 120,
        weight: 2480,
        quantity: 25,
        location: "B-08-B",
        coilId: "AL-48-125-004",
        picked: false,
        packed: false,
        qualityChecked: false
      }
    ]
  },
  {
    id: "PL-2024-0091",
    workOrderId: "WO-2024-0158",
    customerName: "Industrial Fabricators",
    orderNumber: "IF-9923",
    destination: "Customer Pickup",
    priority: "urgent",
    status: "packed",
    dueDate: "2024-01-17",
    assignedTo: "Maria Santos",
    totalWeight: 5520,
    createdAt: "2024-01-14T14:30:00Z",
    completedAt: "2024-01-15T11:45:00Z",
    packingNotes: "Customer requested extra protection wrap",
    lineItems: [
      {
        id: "LI-005",
        material: "Stainless Steel",
        gauge: "14ga",
        width: 36,
        length: 120,
        weight: 3680,
        color: "Mill Finish",
        quantity: 15,
        location: "C-15-A",
        coilId: "SS-36-14-005",
        picked: true,
        pickedBy: "Maria Santos",
        pickedAt: "2024-01-15T08:30:00Z",
        packed: true,
        packedBy: "Maria Santos",
        packedAt: "2024-01-15T10:15:00Z",
        packingMaterial: "Protective Wrap",
        qualityChecked: true,
        qualityCheckedBy: "Maria Santos",
        qualityCheckedAt: "2024-01-15T08:45:00Z",
        actualWeight: 3680
      },
      {
        id: "LI-006",
        material: "Stainless Steel",
        gauge: "16ga",
        width: 36,
        length: 96,
        weight: 1840,
        color: "Mill Finish",
        quantity: 10,
        location: "C-15-B",
        coilId: "SS-36-16-006",
        picked: true,
        pickedBy: "Maria Santos",
        pickedAt: "2024-01-15T09:00:00Z",
        packed: true,
        packedBy: "Maria Santos",
        packedAt: "2024-01-15T10:30:00Z",
        packingMaterial: "Protective Wrap",
        qualityChecked: true,
        qualityCheckedBy: "Maria Santos",
        qualityCheckedAt: "2024-01-15T09:15:00Z",
        actualWeight: 1840
      }
    ]
  }
];

const getPriorityColor = (priority: string) => {
  switch (priority) {
    case "urgent": return "bg-red-100 text-red-800 border-red-200";
    case "high": return "bg-orange-100 text-orange-800 border-orange-200";
    case "normal": return "bg-blue-100 text-blue-800 border-blue-200";
    case "low": return "bg-gray-100 text-gray-800 border-gray-200";
    default: return "bg-gray-100 text-gray-800 border-gray-200";
  }
};

export function Pulling() {
  const [activeTab, setActiveTab] = useState("picking-lists");
  const [searchTerm, setSearchTerm] = useState("");
  const [selectedDestination, setSelectedDestination] = useState("all");
  const [selectedStatus, setSelectedStatus] = useState("all");
  const [selectedPickingList, setSelectedPickingList] = useState<PickingList | null>(null);
  const [selectedItem, setSelectedItem] = useState<LineItem | null>(null);
  const [showPickingDialog, setShowPickingDialog] = useState(false);
  const [showPackingDialog, setShowPackingDialog] = useState(false);
  const [showQualityDialog, setShowQualityDialog] = useState(false);
  const [packingForm, setPackingForm] = useState({
    material: "",
    notes: "",
    actualWeight: ""
  });

  const filteredPickingLists = samplePickingLists.filter(pl => {
    const matchesSearch = pl.customerName.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         pl.orderNumber.toLowerCase().includes(searchTerm.toLowerCase()) ||
                         pl.id.toLowerCase().includes(searchTerm.toLowerCase());
    const matchesDestination = selectedDestination === "all" || pl.destination === selectedDestination;
    const matchesStatus = selectedStatus === "all" || pl.status === selectedStatus;
    return matchesSearch && matchesDestination && matchesStatus;
  });

  const groupedByDestination = filteredPickingLists.reduce((acc, pl) => {
    if (!acc[pl.destination]) acc[pl.destination] = [];
    acc[pl.destination].push(pl);
    return acc;
  }, {} as Record<string, PickingList[]>);

  const handlePickItem = (pickingListId: string, lineItemId: string) => {
    console.log(`Picking item ${lineItemId} from list ${pickingListId}`);
    // Here you would update the picking list
  };

  const handleCompletePickingList = (pickingListId: string) => {
    console.log(`Completing picking list ${pickingListId}`);
    // Here you would update the picking list status
  };

  const handleStartPicking = (item: LineItem) => {
    setSelectedItem(item);
    setShowPickingDialog(true);
  };

  const handleConfirmPick = () => {
    if (selectedItem) {
      console.log(`Confirmed picking of item ${selectedItem.id}`);
      // Update item status to picked
      setShowPickingDialog(false);
      setSelectedItem(null);
    }
  };

  const handleStartPacking = (item: LineItem) => {
    setSelectedItem(item);
    setPackingForm({
      material: "",
      notes: "",
      actualWeight: item.weight.toString()
    });
    setShowPackingDialog(true);
  };

  const handleConfirmPacking = () => {
    if (selectedItem) {
      console.log(`Packed item ${selectedItem.id} with material: ${packingForm.material}`);
      // Update item status to packed
      setShowPackingDialog(false);
      setSelectedItem(null);
      setPackingForm({ material: "", notes: "", actualWeight: "" });
    }
  };

  const handleQualityCheck = (item: LineItem) => {
    setSelectedItem(item);
    setShowQualityDialog(true);
  };

  const handleConfirmQuality = () => {
    if (selectedItem) {
      console.log(`Quality checked item ${selectedItem.id}`);
      // Update item quality status
      setShowQualityDialog(false);
      setSelectedItem(null);
    }
  };

  return (
    <div className="space-y-6">
      <div>
        <h1>Picking & Packing</h1>
        <p className="text-muted-foreground">Manage picking lists and packing operations</p>
      </div>

      <Tabs value={activeTab} onValueChange={setActiveTab} className="space-y-6">
        <TabsList className="grid w-full grid-cols-4">
          <TabsTrigger value="picking-lists" className="flex items-center gap-2">
            <Package className="h-4 w-4" />
            Picking Lists
          </TabsTrigger>
          <TabsTrigger value="picking-process" className="flex items-center gap-2">
            <Scan className="h-4 w-4" />
            Picking Process
          </TabsTrigger>
          <TabsTrigger value="packing-process" className="flex items-center gap-2">
            <Box className="h-4 w-4" />
            Packing Process
          </TabsTrigger>
          <TabsTrigger value="by-destination" className="flex items-center gap-2">
            <MapPin className="h-4 w-4" />
            By Destination
          </TabsTrigger>
        </TabsList>

        {/* Controls */}
        <div className="flex flex-wrap gap-4 items-center justify-between">
          <div className="flex gap-2 items-center">
            <div className="relative">
              <Search className="absolute left-3 top-1/2 transform -translate-y-1/2 h-4 w-4 text-muted-foreground" />
              <Input
                placeholder="Search picking lists..."
                value={searchTerm}
                onChange={(e) => setSearchTerm(e.target.value)}
                className="pl-10 w-64"
              />
            </div>
            <Select value={selectedDestination} onValueChange={setSelectedDestination}>
              <SelectTrigger className="w-48">
                <SelectValue placeholder="Filter by destination" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Destinations</SelectItem>
                {destinations.map(dest => (
                  <SelectItem key={dest} value={dest}>{dest}</SelectItem>
                ))}
              </SelectContent>
            </Select>
            <Select value={selectedStatus} onValueChange={setSelectedStatus}>
              <SelectTrigger className="w-40">
                <SelectValue placeholder="Filter by status" />
              </SelectTrigger>
              <SelectContent>
                <SelectItem value="all">All Status</SelectItem>
                <SelectItem value="ready">Ready</SelectItem>
                <SelectItem value="picking">Picking</SelectItem>
                <SelectItem value="picked">Picked</SelectItem>
                <SelectItem value="packed">Packed</SelectItem>
              </SelectContent>
            </Select>
          </div>

          <Button>
            <Scan className="h-4 w-4 mr-2" />
            Scan Barcode
          </Button>
        </div>

        <TabsContent value="picking-lists" className="space-y-4">
          <div className="grid gap-4">
            {filteredPickingLists.map(pickingList => (
              <Card key={pickingList.id} className="cursor-pointer hover:shadow-md transition-shadow">
                <CardHeader className="pb-3">
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <Package className="h-5 w-5" />
                        {pickingList.id}
                      </CardTitle>
                      <div className="flex items-center gap-4 mt-2 text-sm text-muted-foreground">
                        <span>{pickingList.customerName}</span>
                        <span>•</span>
                        <span>{pickingList.orderNumber}</span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <MapPin className="h-3 w-3" />
                          {pickingList.destination}
                        </span>
                        <span>•</span>
                        <span className="flex items-center gap-1">
                          <Clock className="h-3 w-3" />
                          Due {pickingList.dueDate}
                        </span>
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <Badge variant="outline" className={getPriorityColor(pickingList.priority)}>
                        {pickingList.priority}
                      </Badge>
                      <StatusChip status={pickingList.status} />
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-3">
                    {pickingList.assignedTo && (
                      <div className="flex items-center gap-2 text-sm text-muted-foreground">
                        <User className="h-4 w-4" />
                        Assigned to {pickingList.assignedTo}
                      </div>
                    )}
                    
                    <div className="space-y-2">
                      {pickingList.lineItems.map(item => (
                        <div key={item.id} className="flex items-center justify-between p-3 bg-gray-50 rounded-lg">
                          <div className="flex items-center gap-3">
                            <Checkbox 
                              checked={item.picked}
                              onCheckedChange={() => handlePickItem(pickingList.id, item.id)}
                            />
                            <div>
                              <p className="text-sm font-medium">
                                {item.material} - {item.gauge} - {item.width}"×{item.length}"
                              </p>
                              <p className="text-xs text-muted-foreground">
                                Qty: {item.quantity} • {item.weight} lbs • Location: {item.location}
                                {item.coilId && ` • Coil: ${item.coilId}`}
                              </p>
                              {item.picked && item.pickedBy && (
                                <p className="text-xs text-green-600">
                                  ✓ Picked by {item.pickedBy} at {new Date(item.pickedAt || "").toLocaleTimeString()}
                                </p>
                              )}
                            </div>
                          </div>
                          {!item.picked && (
                            <Button variant="outline" size="sm">
                              <Scan className="h-4 w-4" />
                            </Button>
                          )}
                        </div>
                      ))}
                    </div>

                    <div className="flex items-center justify-between pt-3 border-t">
                      <div className="text-sm text-muted-foreground">
                        Total Weight: {pickingList.totalWeight} lbs •{' '}
                        Progress: {pickingList.lineItems.filter(li => li.picked).length}/{pickingList.lineItems.length} items
                      </div>
                      <div className="flex gap-2">
                        {pickingList.status === 'picking' && (
                          <>
                            <Button variant="outline" size="sm">
                              <RotateCcw className="h-4 w-4 mr-2" />
                              Reset
                            </Button>
                            <Button size="sm" disabled={!pickingList.lineItems.every(li => li.picked)}>
                              <CheckCircle2 className="h-4 w-4 mr-2" />
                              Complete Picking
                            </Button>
                          </>
                        )}
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="picking-process" className="space-y-4">
          <div className="grid gap-4">
            {filteredPickingLists
              .filter(pl => pl.status === 'ready' || pl.status === 'picking')
              .map(pickingList => (
              <Card key={pickingList.id}>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <Scan className="h-5 w-5" />
                        {pickingList.id} - Picking Process
                      </CardTitle>
                      <div className="text-sm text-muted-foreground mt-1">
                        {pickingList.customerName} • {pickingList.orderNumber} • {pickingList.destination}
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <StatusChip status={pickingList.status} />
                      <Badge variant="outline" className={getPriorityColor(pickingList.priority)}>
                        {pickingList.priority}
                      </Badge>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="flex items-center justify-between p-3 bg-teal-50 rounded-lg border border-teal-200">
                      <div className="flex items-center gap-3">
                        <Clipboard className="h-5 w-5 text-teal-600" />
                        <div>
                          <p className="font-medium text-teal-800">Picking Progress</p>
                          <p className="text-sm text-teal-600">
                            {pickingList.lineItems.filter(li => li.picked).length} of {pickingList.lineItems.length} items picked
                          </p>
                        </div>
                      </div>
                      <Progress 
                        value={(pickingList.lineItems.filter(li => li.picked).length / pickingList.lineItems.length) * 100} 
                        className="w-32"
                      />
                    </div>

                    <div className="space-y-3">
                      {pickingList.lineItems.map(item => (
                        <div key={item.id} className={`p-4 rounded-lg border-2 transition-all ${
                          item.picked 
                            ? 'bg-green-50 border-green-200' 
                            : 'bg-white border-gray-200 hover:border-teal-300'
                        }`}>
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-4">
                              <div className={`p-2 rounded-full ${
                                item.picked ? 'bg-green-100' : 'bg-gray-100'
                              }`}>
                                {item.picked ? (
                                  <CheckCircle2 className="h-5 w-5 text-green-600" />
                                ) : (
                                  <Package className="h-5 w-5 text-gray-500" />
                                )}
                              </div>
                              <div className="flex-1">
                                <p className="font-medium text-lg">
                                  {item.material} - {item.gauge}
                                </p>
                                <p className="text-sm text-muted-foreground">
                                  {item.width}"×{item.length}" • Qty: {item.quantity} • {item.weight} lbs
                                </p>
                                <div className="flex items-center gap-4 mt-1 text-sm">
                                  <span className="flex items-center gap-1">
                                    <MapPin className="h-3 w-3" />
                                    Location: {item.location}
                                  </span>
                                  {item.coilId && (
                                    <span className="flex items-center gap-1">
                                      <QrCode className="h-3 w-3" />
                                      Coil: {item.coilId}
                                    </span>
                                  )}
                                </div>
                                {item.picked && item.pickedBy && (
                                  <p className="text-sm text-green-600 mt-2 flex items-center gap-1">
                                    <CheckCircle2 className="h-3 w-3" />
                                    Picked by {item.pickedBy} at {new Date(item.pickedAt || "").toLocaleTimeString()}
                                  </p>
                                )}
                              </div>
                            </div>
                            <div className="flex items-center gap-2">
                              {!item.picked ? (
                                <>
                                  <Button variant="outline" size="sm">
                                    <QrCode className="h-4 w-4 mr-2" />
                                    Scan
                                  </Button>
                                  <Button 
                                    size="sm" 
                                    onClick={() => handleStartPicking(item)}
                                    className="bg-teal-600 hover:bg-teal-700"
                                  >
                                    <Play className="h-4 w-4 mr-2" />
                                    Start Pick
                                  </Button>
                                </>
                              ) : (
                                <div className="flex items-center gap-2">
                                  {!item.qualityChecked && (
                                    <Button 
                                      variant="outline" 
                                      size="sm"
                                      onClick={() => handleQualityCheck(item)}
                                    >
                                      <Shield className="h-4 w-4 mr-2" />
                                      Quality Check
                                    </Button>
                                  )}
                                  <Button variant="outline" size="sm">
                                    <Eye className="h-4 w-4 mr-2" />
                                    Details
                                  </Button>
                                </div>
                              )}
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>

                    <div className="flex items-center justify-between pt-4 border-t">
                      <div className="text-sm text-muted-foreground">
                        Total Weight: {pickingList.totalWeight} lbs
                      </div>
                      <div className="flex gap-2">
                        <Button variant="outline" size="sm">
                          <Pause className="h-4 w-4 mr-2" />
                          Pause List
                        </Button>
                        <Button 
                          size="sm" 
                          disabled={!pickingList.lineItems.every(li => li.picked)}
                          className="bg-teal-600 hover:bg-teal-700"
                        >
                          <CheckCircle2 className="h-4 w-4 mr-2" />
                          Complete Picking
                        </Button>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="packing-process" className="space-y-4">
          <div className="grid gap-4">
            {filteredPickingLists
              .filter(pl => pl.status === 'picked' || pl.status === 'packing' || pl.status === 'packed')
              .map(pickingList => (
              <Card key={pickingList.id}>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <Box className="h-5 w-5" />
                        {pickingList.id} - Packing Process
                      </CardTitle>
                      <div className="text-sm text-muted-foreground mt-1">
                        {pickingList.customerName} • {pickingList.orderNumber} • {pickingList.destination}
                      </div>
                    </div>
                    <div className="flex items-center gap-2">
                      <StatusChip status={pickingList.status} />
                      <Badge variant="outline" className={getPriorityColor(pickingList.priority)}>
                        {pickingList.priority}
                      </Badge>
                    </div>
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="flex items-center justify-between p-3 bg-blue-50 rounded-lg border border-blue-200">
                      <div className="flex items-center gap-3">
                        <Box className="h-5 w-5 text-blue-600" />
                        <div>
                          <p className="font-medium text-blue-800">Packing Progress</p>
                          <p className="text-sm text-blue-600">
                            {pickingList.lineItems.filter(li => li.packed).length} of {pickingList.lineItems.length} items packed
                          </p>
                        </div>
                      </div>
                      <Progress 
                        value={(pickingList.lineItems.filter(li => li.packed).length / pickingList.lineItems.length) * 100} 
                        className="w-32"
                      />
                    </div>

                    <div className="space-y-3">
                      {pickingList.lineItems
                        .filter(item => item.picked)
                        .map(item => (
                        <div key={item.id} className={`p-4 rounded-lg border-2 transition-all ${
                          item.packed 
                            ? 'bg-blue-50 border-blue-200' 
                            : 'bg-yellow-50 border-yellow-200 hover:border-blue-300'
                        }`}>
                          <div className="flex items-center justify-between">
                            <div className="flex items-center gap-4">
                              <div className={`p-2 rounded-full ${
                                item.packed ? 'bg-blue-100' : 'bg-yellow-100'
                              }`}>
                                {item.packed ? (
                                  <CheckCircle2 className="h-5 w-5 text-blue-600" />
                                ) : (
                                  <Box className="h-5 w-5 text-yellow-600" />
                                )}
                              </div>
                              <div className="flex-1">
                                <p className="font-medium text-lg">
                                  {item.material} - {item.gauge}
                                </p>
                                <p className="text-sm text-muted-foreground">
                                  {item.width}"×{item.length}" • Qty: {item.quantity} • {item.weight} lbs
                                </p>
                                <div className="flex items-center gap-4 mt-1 text-sm">
                                  <span className="flex items-center gap-1">
                                    <Weight className="h-3 w-3" />
                                    Target: {item.weight} lbs
                                    {item.actualWeight && ` • Actual: ${item.actualWeight} lbs`}
                                  </span>
                                  {!item.qualityChecked && (
                                    <Badge variant="outline" className="bg-yellow-100 text-yellow-800 border-yellow-300">
                                      Quality Check Required
                                    </Badge>
                                  )}
                                </div>
                                {item.packed && item.packedBy && (
                                  <div className="mt-2 space-y-1">
                                    <p className="text-sm text-blue-600 flex items-center gap-1">
                                      <CheckCircle2 className="h-3 w-3" />
                                      Packed by {item.packedBy} at {new Date(item.packedAt || "").toLocaleTimeString()}
                                    </p>
                                    {item.packingMaterial && (
                                      <p className="text-sm text-muted-foreground">
                                        Material: {item.packingMaterial}
                                      </p>
                                    )}
                                    {item.packingNotes && (
                                      <p className="text-sm text-muted-foreground">
                                        Notes: {item.packingNotes}
                                      </p>
                                    )}
                                  </div>
                                )}
                              </div>
                            </div>
                            <div className="flex items-center gap-2">
                              {!item.packed ? (
                                <>
                                  {!item.qualityChecked && (
                                    <Button 
                                      variant="outline" 
                                      size="sm"
                                      onClick={() => handleQualityCheck(item)}
                                    >
                                      <Shield className="h-4 w-4 mr-2" />
                                      Quality Check
                                    </Button>
                                  )}
                                  <Button 
                                    size="sm" 
                                    onClick={() => handleStartPacking(item)}
                                    disabled={!item.qualityChecked}
                                    className="bg-blue-600 hover:bg-blue-700"
                                  >
                                    <Play className="h-4 w-4 mr-2" />
                                    Start Pack
                                  </Button>
                                </>
                              ) : (
                                <div className="flex items-center gap-2">
                                  <Button variant="outline" size="sm">
                                    <PrinterIcon className="h-4 w-4 mr-2" />
                                    Print Label
                                  </Button>
                                  <Button variant="outline" size="sm">
                                    <Eye className="h-4 w-4 mr-2" />
                                    Details
                                  </Button>
                                </div>
                              )}
                            </div>
                          </div>
                        </div>
                      ))}
                    </div>

                    <div className="flex items-center justify-between pt-4 border-t">
                      <div className="text-sm text-muted-foreground">
                        Total Weight: {pickingList.totalWeight} lbs
                      </div>
                      <div className="flex gap-2">
                        <Button variant="outline" size="sm">
                          <Save className="h-4 w-4 mr-2" />
                          Save Progress
                        </Button>
                        <Button 
                          size="sm" 
                          disabled={!pickingList.lineItems.filter(li => li.picked).every(li => li.packed)}
                          className="bg-blue-600 hover:bg-blue-700"
                        >
                          <Truck className="h-4 w-4 mr-2" />
                          Complete Packing
                        </Button>
                      </div>
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="by-destination" className="space-y-4">
          <div className="grid gap-6">
            {Object.entries(groupedByDestination).map(([destination, lists]) => (
              <Card key={destination}>
                <CardHeader>
                  <CardTitle className="flex items-center gap-2">
                    <MapPin className="h-5 w-5" />
                    {destination}
                    <Badge variant="secondary">{lists.length}</Badge>
                  </CardTitle>
                </CardHeader>
                <CardContent>
                  <div className="grid gap-3">
                    {lists.map(list => (
                      <div key={list.id} className="flex items-center justify-between p-3 border rounded-lg">
                        <div>
                          <p className="font-medium">{list.id} - {list.customerName}</p>
                          <p className="text-sm text-muted-foreground">
                            {list.orderNumber} • {list.totalWeight} lbs • Due {list.dueDate}
                          </p>
                        </div>
                        <div className="flex items-center gap-2">
                          <Badge variant="outline" className={getPriorityColor(list.priority)}>
                            {list.priority}
                          </Badge>
                          <StatusChip status={list.status} />
                        </div>
                      </div>
                    ))}
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>

        <TabsContent value="packing" className="space-y-4">
          <div className="grid gap-4">
            {filteredPickingLists
              .filter(pl => pl.status === 'picked' || pl.status === 'packed')
              .map(pickingList => (
              <Card key={pickingList.id}>
                <CardHeader>
                  <div className="flex items-start justify-between">
                    <div>
                      <CardTitle className="text-lg flex items-center gap-2">
                        <Package className="h-5 w-5" />
                        {pickingList.id}
                      </CardTitle>
                      <div className="text-sm text-muted-foreground mt-1">
                        {pickingList.customerName} • {pickingList.orderNumber}
                      </div>
                    </div>
                    <StatusChip status={pickingList.status} />
                  </div>
                </CardHeader>
                <CardContent>
                  <div className="space-y-4">
                    <div className="grid grid-cols-2 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">Destination:</span>
                        <p className="font-medium">{pickingList.destination}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Total Weight:</span>
                        <p className="font-medium">{pickingList.totalWeight} lbs</p>
                      </div>
                    </div>

                    {pickingList.packingNotes && (
                      <div>
                        <span className="text-muted-foreground text-sm">Packing Notes:</span>
                        <p className="text-sm bg-yellow-50 p-2 rounded border">
                          {pickingList.packingNotes}
                        </p>
                      </div>
                    )}

                    <div className="flex justify-end gap-2">
                      {pickingList.status === 'picked' && (
                        <Button onClick={() => handleCompletePickingList(pickingList.id)}>
                          <Truck className="h-4 w-4 mr-2" />
                          Mark as Packed
                        </Button>
                      )}
                    </div>
                  </div>
                </CardContent>
              </Card>
            ))}
          </div>
        </TabsContent>
      </Tabs>

      {/* Summary Stats */}
      <div className="grid grid-cols-1 md:grid-cols-4 gap-4">
        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center">
              <div className="p-2 bg-blue-100 rounded-full">
                <Package className="h-4 w-4 text-blue-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm text-muted-foreground">Active Lists</p>
                <p className="text-2xl font-medium">
                  {samplePickingLists.filter(pl => ['ready', 'picking'].includes(pl.status)).length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>

        <Card>
          <CardContent className="pt-6">
            <div className="flex items-center">
              <div className="p-2 bg-orange-100 rounded-full">
                <Clock className="h-4 w-4 text-orange-600" />
              </div>
              <div className="ml-4">
                <p className="text-sm text-muted-foreground">In Progress</p>
                <p className="text-2xl font-medium">
                  {samplePickingLists.filter(pl => pl.status === 'picking').length}
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
                <p className="text-sm text-muted-foreground">Completed</p>
                <p className="text-2xl font-medium">
                  {samplePickingLists.filter(pl => ['packed', 'shipped'].includes(pl.status)).length}
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
                <p className="text-2xl font-medium">
                  {samplePickingLists.filter(pl => 
                    new Date(pl.dueDate) < new Date() && !['packed', 'shipped'].includes(pl.status)
                  ).length}
                </p>
              </div>
            </div>
          </CardContent>
        </Card>
      </div>

      {/* Picking Process Dialog */}
      <Dialog open={showPickingDialog} onOpenChange={setShowPickingDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Scan className="h-5 w-5" />
              Pick Item - {selectedItem?.material}
            </DialogTitle>
          </DialogHeader>
          {selectedItem && (
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4 p-4 bg-gray-50 rounded-lg">
                <div>
                  <label className="text-sm text-muted-foreground">Material</label>
                  <p className="font-medium">{selectedItem.material}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Gauge</label>
                  <p className="font-medium">{selectedItem.gauge}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Dimensions</label>
                  <p className="font-medium">{selectedItem.width}"×{selectedItem.length}"</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Quantity</label>
                  <p className="font-medium">{selectedItem.quantity}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Location</label>
                  <p className="font-medium">{selectedItem.location}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Expected Weight</label>
                  <p className="font-medium">{selectedItem.weight} lbs</p>
                </div>
              </div>

              <div className="space-y-4">
                <div className="text-center">
                  <div className="inline-flex items-center justify-center w-32 h-32 bg-teal-100 rounded-full mb-4">
                    <QrCode className="h-16 w-16 text-teal-600" />
                  </div>
                  <p className="text-sm text-muted-foreground">Scan the barcode or QR code on the material</p>
                </div>

                <div className="flex gap-4">
                  <Input 
                    placeholder="Scan or enter barcode/coil ID..."
                    className="text-center text-lg"
                  />
                  <Button variant="outline">
                    <Scan className="h-4 w-4" />
                  </Button>
                </div>
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t">
                <Button variant="outline" onClick={() => setShowPickingDialog(false)}>
                  Cancel
                </Button>
                <Button onClick={handleConfirmPick} className="bg-teal-600 hover:bg-teal-700">
                  <CheckCircle2 className="h-4 w-4 mr-2" />
                  Confirm Pick
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Packing Process Dialog */}
      <Dialog open={showPackingDialog} onOpenChange={setShowPackingDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Box className="h-5 w-5" />
              Pack Item - {selectedItem?.material}
            </DialogTitle>
          </DialogHeader>
          {selectedItem && (
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4 p-4 bg-gray-50 rounded-lg">
                <div>
                  <label className="text-sm text-muted-foreground">Material</label>
                  <p className="font-medium">{selectedItem.material}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Dimensions</label>
                  <p className="font-medium">{selectedItem.width}"×{selectedItem.length}"</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Quantity</label>
                  <p className="font-medium">{selectedItem.quantity}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Expected Weight</label>
                  <p className="font-medium">{selectedItem.weight} lbs</p>
                </div>
              </div>

              <div className="space-y-4">
                <div>
                  <label className="text-sm font-medium">Packing Material</label>
                  <Select value={packingForm.material} onValueChange={(value) => setPackingForm({...packingForm, material: value})}>
                    <SelectTrigger>
                      <SelectValue placeholder="Select packing material..." />
                    </SelectTrigger>
                    <SelectContent>
                      {packingMaterials.map(material => (
                        <SelectItem key={material} value={material}>{material}</SelectItem>
                      ))}
                    </SelectContent>
                  </Select>
                </div>

                <div>
                  <label className="text-sm font-medium">Actual Weight (lbs)</label>
                  <Input 
                    type="number"
                    value={packingForm.actualWeight}
                    onChange={(e) => setPackingForm({...packingForm, actualWeight: e.target.value})}
                    placeholder="Enter actual weight..."
                  />
                </div>

                <div>
                  <label className="text-sm font-medium">Packing Notes (Optional)</label>
                  <Textarea 
                    value={packingForm.notes}
                    onChange={(e) => setPackingForm({...packingForm, notes: e.target.value})}
                    placeholder="Any special packing instructions or notes..."
                    rows={3}
                  />
                </div>

                <div className="flex items-center gap-2 p-3 bg-yellow-50 rounded-lg border border-yellow-200">
                  <AlertCircle className="h-5 w-5 text-yellow-600" />
                  <p className="text-sm text-yellow-800">
                    Ensure item has passed quality check before packing
                  </p>
                </div>
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t">
                <Button variant="outline" onClick={() => setShowPackingDialog(false)}>
                  Cancel
                </Button>
                <Button 
                  onClick={handleConfirmPacking} 
                  disabled={!packingForm.material || !packingForm.actualWeight}
                  className="bg-blue-600 hover:bg-blue-700"
                >
                  <CheckCircle2 className="h-4 w-4 mr-2" />
                  Confirm Packing
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>

      {/* Quality Check Dialog */}
      <Dialog open={showQualityDialog} onOpenChange={setShowQualityDialog}>
        <DialogContent className="max-w-2xl">
          <DialogHeader>
            <DialogTitle className="flex items-center gap-2">
              <Shield className="h-5 w-5" />
              Quality Check - {selectedItem?.material}
            </DialogTitle>
          </DialogHeader>
          {selectedItem && (
            <div className="space-y-6">
              <div className="grid grid-cols-2 gap-4 p-4 bg-gray-50 rounded-lg">
                <div>
                  <label className="text-sm text-muted-foreground">Material</label>
                  <p className="font-medium">{selectedItem.material}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Gauge</label>
                  <p className="font-medium">{selectedItem.gauge}</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Dimensions</label>
                  <p className="font-medium">{selectedItem.width}"×{selectedItem.length}"</p>
                </div>
                <div>
                  <label className="text-sm text-muted-foreground">Expected Weight</label>
                  <p className="font-medium">{selectedItem.weight} lbs</p>
                </div>
              </div>

              <div className="space-y-4">
                <h3 className="font-medium">Quality Checklist</h3>
                <div className="space-y-3">
                  {[
                    "Material condition - No visible damage or defects",
                    "Dimensions match specifications",
                    "Surface finish meets requirements",
                    "No rust, scratches, or contamination",
                    "Correct gauge/thickness verified",
                    "Quantity count accurate"
                  ].map((check, index) => (
                    <div key={index} className="flex items-center gap-3 p-3 border rounded-lg">
                      <Checkbox />
                      <span className="text-sm">{check}</span>
                    </div>
                  ))}
                </div>

                <div>
                  <label className="text-sm font-medium">Quality Notes (Optional)</label>
                  <Textarea 
                    placeholder="Record any quality observations or issues..."
                    rows={3}
                  />
                </div>
              </div>

              <div className="flex justify-end gap-2 pt-4 border-t">
                <Button variant="outline" onClick={() => setShowQualityDialog(false)}>
                  Cancel
                </Button>
                <Button onClick={handleConfirmQuality} className="bg-green-600 hover:bg-green-700">
                  <Shield className="h-4 w-4 mr-2" />
                  Pass Quality Check
                </Button>
              </div>
            </div>
          )}
        </DialogContent>
      </Dialog>
    </div>
  );
}