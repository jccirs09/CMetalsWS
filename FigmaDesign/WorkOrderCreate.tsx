import { useState, useEffect } from "react";
import { Card, CardContent, CardDescription, CardHeader, CardTitle } from "./ui/card";
import { Button } from "./ui/button";
import { Badge } from "./ui/badge";
import { Input } from "./ui/input";
import { Label } from "./ui/label";
import { Textarea } from "./ui/textarea";
import { Checkbox } from "./ui/checkbox";
import {
  Select,
  SelectContent,
  SelectItem,
  SelectTrigger,
  SelectValue,
} from "./ui/select";
import { Separator } from "./ui/separator";
import { ScrollArea } from "./ui/scroll-area";
import {
  Calendar,
  Save,
  ArrowLeft,
  ArrowRight,
  CheckCircle,
  Package,
  Settings,
  Clock,
  User,
  Building2,
  FileText,
  Tag,
  ShoppingCart,
  ListChecks,
  Eye,
  MapPin,
  Truck
} from "lucide-react";
import { toast } from "sonner@2.0.3";

interface WorkOrderCreateProps {
  onNavigate?: (page: string) => void;
}

// Enums matching C# entities
enum WorkOrderStatus {
  Draft = "Draft",
  Pending = "Pending", 
  InProgress = "InProgress",
  Completed = "Completed",
  Canceled = "Canceled",
  Awaiting = "Awaiting"
}

enum MachineCategory {
  CTL = "CTL",
  Slitter = "Slitter", 
  Picking = "Picking",
  Packing = "Packing",
  Crane = "Crane"
}

// Interfaces matching C# entities
interface Branch {
  id: number;
  name: string;
  code: string;
}

interface Machine {
  id: number;
  name: string;
  category: MachineCategory;
  isActive: boolean;
  estimatedLbsPerHour: number;
}

interface SalesOrder {
  id: string;
  orderNumber: string;
  customerName: string;
  customerPO?: string;
  dueDate: string;
  totalWeight: number;
  status: string;
  priority: "low" | "normal" | "high" | "urgent";
}

interface OrderLineItem {
  id: string;
  salesOrderId: string;
  itemCode: string;
  description: string;
  orderQuantity: number;
  orderWeight: number;
  width?: number;
  length?: number;
  unit: string;
  location?: string;
  isStockItem: boolean;
  remainingQuantity: number;
  remainingWeight: number;
}

interface WorkOrderItem {
  id: number;
  workOrderId: number;
  pickingListItemId?: number;
  itemCode: string;
  description: string;
  salesOrderNumber?: string;
  customerName?: string;
  orderQuantity?: number;
  orderWeight?: number;
  width?: number;
  length?: number;
  producedQuantity?: number;
  producedWeight?: number;
  unit?: string;
  location?: string;
  isStockItem: boolean;
  originalOrderLineItemId: string;
}

interface WorkOrderForm {
  // Step 1: Machine Selection
  machineId?: number;
  machine?: Machine;
  
  // Step 2: Tag Number
  tagNumber: string;
  
  // Step 3: Order Selection
  selectedSalesOrders: string[];
  salesOrders: SalesOrder[];
  
  // Step 4: Notes/Instructions
  instructions?: string;
  priority: "low" | "normal" | "high" | "urgent";
  
  // Step 5: Line Items
  availableLineItems: OrderLineItem[];
  selectedLineItems: WorkOrderItem[];
  
  // Step 6: Scheduling (Smart)
  scheduledDate: string;
  scheduledStartTime?: string;
  scheduledEndTime?: string;
  estimatedDuration?: number;
  
  // Auto-generated
  workOrderNumber?: string;
  branchId: number;
  machineCategory: MachineCategory;
  status: WorkOrderStatus;
  createdBy: string;
}

// Mock data
const branches: Branch[] = [
  { id: 1, name: "Main Facility", code: "MAIN" },
  { id: 2, name: "East Warehouse", code: "EAST" }
];

const machines: Machine[] = [
  { id: 1, name: "CTL Line 1", category: MachineCategory.CTL, isActive: true, estimatedLbsPerHour: 1200 },
  { id: 2, name: "CTL Line 2", category: MachineCategory.CTL, isActive: true, estimatedLbsPerHour: 1100 },
  { id: 3, name: "Slitter 1", category: MachineCategory.Slitter, isActive: true, estimatedLbsPerHour: 800 },
  { id: 4, name: "Slitter 2", category: MachineCategory.Slitter, isActive: true, estimatedLbsPerHour: 900 },
  { id: 5, name: "Picking Line 1", category: MachineCategory.Picking, isActive: true, estimatedLbsPerHour: 1500 },
  { id: 6, name: "Packing Station 1", category: MachineCategory.Packing, isActive: true, estimatedLbsPerHour: 1000 }
];

const mockSalesOrders: SalesOrder[] = [
  {
    id: "SO-45621",
    orderNumber: "SO-45621", 
    customerName: "Industrial Metals Co",
    customerPO: "PO-12345",
    dueDate: "2024-12-15",
    totalWeight: 2400,
    status: "Confirmed",
    priority: "normal"
  },
  {
    id: "SO-45622",
    orderNumber: "SO-45622",
    customerName: "Precision Parts LLC", 
    customerPO: "PO-67890",
    dueDate: "2024-12-13",
    totalWeight: 1800,
    status: "Confirmed",
    priority: "high"
  },
  {
    id: "SO-45623",
    orderNumber: "SO-45623",
    customerName: "Metro Construction",
    customerPO: "PO-98765",
    dueDate: "2024-12-14",
    totalWeight: 3200,
    status: "Confirmed", 
    priority: "normal"
  },
  {
    id: "SO-45624",
    orderNumber: "SO-45624",
    customerName: "Steel Solutions Inc",
    dueDate: "2024-12-16",
    totalWeight: 2100,
    status: "Confirmed",
    priority: "low"
  }
];

const mockOrderLineItems: OrderLineItem[] = [
  {
    id: "LI-001",
    salesOrderId: "SO-45621",
    itemCode: "HR-16-48-120",
    description: "Hot Rolled Coil - 16 GA x 48\" x 120\"", 
    orderQuantity: 1,
    orderWeight: 2400,
    width: 48,
    length: 120,
    unit: "PCS",
    location: "A-12-03",
    isStockItem: false,
    remainingQuantity: 1,
    remainingWeight: 2400
  },
  {
    id: "LI-002", 
    salesOrderId: "SO-45622",
    itemCode: "CR-20-24-96",
    description: "Cold Rolled Sheet - 20 GA x 24\" x 96\"",
    orderQuantity: 5,
    orderWeight: 1800,
    width: 24,
    length: 96,
    unit: "PCS",
    location: "B-05-12",
    isStockItem: false,
    remainingQuantity: 5,
    remainingWeight: 1800
  },
  {
    id: "LI-003",
    salesOrderId: "SO-45623", 
    itemCode: "HR-14-60-144",
    description: "Hot Rolled Coil - 14 GA x 60\" x 144\"",
    orderQuantity: 1,
    orderWeight: 3200,
    width: 60,
    length: 144,
    unit: "PCS",
    location: "A-15-01",
    isStockItem: false,
    remainingQuantity: 1,
    remainingWeight: 3200
  },
  {
    id: "LI-004",
    salesOrderId: "SO-45624",
    itemCode: "MIXED-001", 
    description: "Galvanized Sheet - 18 GA x 36\" x 72\"",
    orderQuantity: 3,
    orderWeight: 890,
    width: 36,
    length: 72,
    unit: "PCS",
    location: "C-08-15",
    isStockItem: true,
    remainingQuantity: 3,
    remainingWeight: 890
  },
  {
    id: "LI-005",
    salesOrderId: "SO-45624",
    itemCode: "MIXED-002",
    description: "Aluminum Sheet - 16 GA x 48\" x 96\"",
    orderQuantity: 2,
    orderWeight: 1210,
    width: 48,
    length: 96,
    unit: "PCS", 
    location: "D-03-22",
    isStockItem: true,
    remainingQuantity: 2,
    remainingWeight: 1210
  }
];

const initialForm: WorkOrderForm = {
  tagNumber: "",
  selectedSalesOrders: [],
  salesOrders: mockSalesOrders,
  instructions: "",
  priority: "normal",
  availableLineItems: [],
  selectedLineItems: [],
  scheduledDate: "",
  branchId: 1,
  machineCategory: MachineCategory.CTL,
  status: WorkOrderStatus.Draft,
  createdBy: "Current User"
};

const steps = [
  { id: 1, name: "Machine", icon: Settings, description: "Select production machine" },
  { id: 2, name: "Tag Number", icon: Tag, description: "Enter work order tag" },
  { id: 3, name: "Orders", icon: ShoppingCart, description: "Select sales orders" },
  { id: 4, name: "Instructions", icon: FileText, description: "Add notes and priority" },
  { id: 5, name: "Line Items", icon: ListChecks, description: "Select items to produce" },
  { id: 6, name: "Review", icon: Eye, description: "Review and create" }
];

export function WorkOrderCreate({ onNavigate }: WorkOrderCreateProps = {}) {
  const [currentStep, setCurrentStep] = useState(1);
  const [form, setForm] = useState<WorkOrderForm>(initialForm);

  const updateForm = (field: keyof WorkOrderForm, value: any) => {
    setForm(prev => ({ ...prev, [field]: value }));
  };

  // Smart scheduling function
  const calculateSmartSchedule = (selectedDate: string, machineId: number, totalWeight: number) => {
    if (!selectedDate || !machineId || !totalWeight) return null;

    const machine = machines.find(m => m.id === machineId);
    if (!machine) return null;

    // Calculate duration based on total weight and machine capacity
    const estimatedHours = Math.ceil(totalWeight / machine.estimatedLbsPerHour);
    
    // For demo: assume next available slot is 9:00 AM on selected date
    const startTime = "09:00";
    const startDateTime = new Date(`${selectedDate}T${startTime}:00`);
    const endDateTime = new Date(startDateTime.getTime() + (estimatedHours * 60 * 60 * 1000));
    
    return {
      startTime: startTime,
      endTime: endDateTime.toTimeString().slice(0, 5),
      duration: estimatedHours
    };
  };

  // Update available line items when orders are selected
  useEffect(() => {
    const availableItems = mockOrderLineItems.filter(item => 
      form.selectedSalesOrders.includes(item.salesOrderId)
    );
    updateForm("availableLineItems", availableItems);
  }, [form.selectedSalesOrders]);

  // Calculate smart schedule when date or machine changes
  useEffect(() => {
    if (form.scheduledDate && form.machineId && form.selectedLineItems.length > 0) {
      const totalWeight = form.selectedLineItems.reduce((sum, item) => sum + (item.orderWeight || 0), 0);
      const schedule = calculateSmartSchedule(form.scheduledDate, form.machineId, totalWeight);
      if (schedule) {
        updateForm("scheduledStartTime", schedule.startTime);
        updateForm("scheduledEndTime", schedule.endTime);
        updateForm("estimatedDuration", schedule.duration);
      }
    }
  }, [form.scheduledDate, form.machineId, form.selectedLineItems]);

  const canProceedToNext = () => {
    switch (currentStep) {
      case 1: return form.machineId !== undefined;
      case 2: return form.tagNumber.trim() !== "";
      case 3: return form.selectedSalesOrders.length > 0;
      case 4: return true; // Instructions are optional
      case 5: return form.selectedLineItems.length > 0;
      case 6: return form.scheduledDate !== "";
      default: return false;
    }
  };

  const nextStep = () => {
    if (canProceedToNext() && currentStep < steps.length) {
      setCurrentStep(currentStep + 1);
    }
  };

  const prevStep = () => {
    if (currentStep > 1) {
      setCurrentStep(currentStep - 1);
    }
  };

  const selectMachine = (machine: Machine) => {
    updateForm("machineId", machine.id);
    updateForm("machine", machine);
    updateForm("machineCategory", machine.category);
  };

  const toggleSalesOrder = (orderId: string) => {
    const selected = form.selectedSalesOrders.includes(orderId);
    if (selected) {
      updateForm("selectedSalesOrders", form.selectedSalesOrders.filter(id => id !== orderId));
    } else {
      updateForm("selectedSalesOrders", [...form.selectedSalesOrders, orderId]);
    }
  };

  const toggleLineItem = (lineItem: OrderLineItem) => {
    const existingIndex = form.selectedLineItems.findIndex(item => item.originalOrderLineItemId === lineItem.id);
    
    if (existingIndex >= 0) {
      // Remove item
      updateForm("selectedLineItems", form.selectedLineItems.filter((_, index) => index !== existingIndex));
    } else {
      // Add item
      const salesOrder = form.salesOrders.find(so => so.id === lineItem.salesOrderId);
      const newWorkOrderItem: WorkOrderItem = {
        id: 0, // Will be set by backend
        workOrderId: 0, // Will be set by backend
        itemCode: lineItem.itemCode,
        description: lineItem.description,
        salesOrderNumber: lineItem.salesOrderId,
        customerName: salesOrder?.customerName,
        orderQuantity: lineItem.orderQuantity,
        orderWeight: lineItem.orderWeight,
        width: lineItem.width,
        length: lineItem.length,
        unit: lineItem.unit,
        location: lineItem.location,
        isStockItem: lineItem.isStockItem,
        originalOrderLineItemId: lineItem.id
      };
      updateForm("selectedLineItems", [...form.selectedLineItems, newWorkOrderItem]);
    }
  };

  const createWorkOrder = () => {
    // Generate work order number
    const woNumber = `WO-${Date.now()}`;
    updateForm("workOrderNumber", woNumber);

    const totalWeight = form.selectedLineItems.reduce((sum, item) => sum + (item.orderWeight || 0), 0);
    
    const workOrder = {
      workOrderNumber: woNumber,
      tagNumber: form.tagNumber,
      branchId: form.branchId,
      machineId: form.machineId,
      machineCategory: form.machineCategory,
      dueDate: form.scheduledDate,
      instructions: form.instructions,
      scheduledStartDate: `${form.scheduledDate}T${form.scheduledStartTime}:00`,
      scheduledEndDate: `${form.scheduledDate}T${form.scheduledEndTime}:00`,
      status: WorkOrderStatus.Draft,
      createdBy: form.createdBy,
      items: form.selectedLineItems,
      priority: form.priority,
      totalWeight
    };

    console.log("Creating work order:", workOrder);
    toast.success(`Work order ${woNumber} created successfully`);
    
    // Reset form and navigate back
    setForm(initialForm);
    setCurrentStep(1);
    onNavigate?.("work-orders");
  };

  const renderStepContent = () => {
    switch (currentStep) {
      case 1:
        return (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Settings className="h-5 w-5" />
                Select Production Machine
              </CardTitle>
              <CardDescription>
                Choose the machine that will process this work order
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="grid grid-cols-1 md:grid-cols-2 lg:grid-cols-3 gap-4">
                {machines.filter(m => m.isActive).map((machine) => (
                  <Card 
                    key={machine.id}
                    className={`cursor-pointer transition-all hover:shadow-md ${
                      form.machineId === machine.id ? 'ring-2 ring-teal-500 bg-teal-50' : ''
                    }`}
                    onClick={() => selectMachine(machine)}
                  >
                    <CardContent className="p-4">
                      <div className="space-y-2">
                        <div className="flex items-center justify-between">
                          <h3 className="font-medium">{machine.name}</h3>
                          <Badge variant="outline">{machine.category}</Badge>
                        </div>
                        <p className="text-sm text-muted-foreground">
                          Capacity: {machine.estimatedLbsPerHour.toLocaleString()} lbs/hr
                        </p>
                        {form.machineId === machine.id && (
                          <div className="flex items-center gap-1 text-teal-600">
                            <CheckCircle className="h-4 w-4" />
                            <span className="text-sm">Selected</span>
                          </div>
                        )}
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>
            </CardContent>
          </Card>
        );

      case 2:
        return (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Tag className="h-5 w-5" />
                Work Order Tag Number
              </CardTitle>
              <CardDescription>
                Enter a unique tag number for this work order
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="tagNumber">Tag Number *</Label>
                <Input
                  id="tagNumber"
                  placeholder="TAG-001"
                  value={form.tagNumber}
                  onChange={(e) => updateForm("tagNumber", e.target.value)}
                  className="text-lg"
                />
                <p className="text-sm text-muted-foreground">
                  Use a unique identifier that will be easily recognizable on the shop floor
                </p>
              </div>
              
              {form.machine && (
                <div className="p-4 bg-gray-50 rounded-lg">
                  <div className="flex items-center gap-2 mb-2">
                    <Settings className="h-4 w-4 text-muted-foreground" />
                    <span className="text-sm text-muted-foreground">Selected Machine:</span>
                  </div>
                  <p className="font-medium">{form.machine.name}</p>
                </div>
              )}
            </CardContent>
          </Card>
        );

      case 3:
        return (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <ShoppingCart className="h-5 w-5" />
                Select Sales Orders
              </CardTitle>
              <CardDescription>
                Choose which sales orders to include in this work order
              </CardDescription>
            </CardHeader>
            <CardContent>
              <div className="space-y-4">
                {form.salesOrders.map((order) => (
                  <Card 
                    key={order.id}
                    className={`cursor-pointer transition-all hover:shadow-md ${
                      form.selectedSalesOrders.includes(order.id) ? 'ring-2 ring-teal-500 bg-teal-50' : ''
                    }`}
                    onClick={() => toggleSalesOrder(order.id)}
                  >
                    <CardContent className="p-4">
                      <div className="flex items-center justify-between">
                        <div className="space-y-1">
                          <div className="flex items-center gap-2">
                            <h3 className="font-medium">{order.orderNumber}</h3>
                            <Badge className={
                              order.priority === "urgent" ? "bg-red-100 text-red-700" :
                              order.priority === "high" ? "bg-orange-100 text-orange-700" :
                              order.priority === "normal" ? "bg-blue-100 text-blue-700" :
                              "bg-gray-100 text-gray-700"
                            }>
                              {order.priority}
                            </Badge>
                          </div>
                          <p className="text-sm text-muted-foreground">{order.customerName}</p>
                          {order.customerPO && (
                            <p className="text-xs text-muted-foreground">PO: {order.customerPO}</p>
                          )}
                        </div>
                        <div className="text-right space-y-1">
                          <p className="text-sm font-medium">{order.totalWeight.toLocaleString()} lbs</p>
                          <p className="text-xs text-muted-foreground">Due: {new Date(order.dueDate).toLocaleDateString()}</p>
                          {form.selectedSalesOrders.includes(order.id) && (
                            <div className="flex items-center gap-1 text-teal-600">
                              <CheckCircle className="h-4 w-4" />
                            </div>
                          )}
                        </div>
                      </div>
                    </CardContent>
                  </Card>
                ))}
              </div>

              {form.selectedSalesOrders.length > 0 && (
                <div className="mt-6 p-4 bg-teal-50 rounded-lg">
                  <h4 className="font-medium mb-2">Selected Orders Summary:</h4>
                  <div className="text-sm text-muted-foreground">
                    {form.selectedSalesOrders.length} order(s) selected • 
                    Total Weight: {form.salesOrders
                      .filter(order => form.selectedSalesOrders.includes(order.id))
                      .reduce((sum, order) => sum + order.totalWeight, 0)
                      .toLocaleString()} lbs
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        );

      case 4:
        return (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <FileText className="h-5 w-5" />
                Instructions & Priority
              </CardTitle>
              <CardDescription>
                Add special instructions and set work order priority
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-4">
              <div className="space-y-2">
                <Label htmlFor="priority">Priority</Label>
                <Select value={form.priority} onValueChange={(value) => updateForm("priority", value)}>
                  <SelectTrigger>
                    <SelectValue />
                  </SelectTrigger>
                  <SelectContent>
                    <SelectItem value="low">Low</SelectItem>
                    <SelectItem value="normal">Normal</SelectItem>
                    <SelectItem value="high">High</SelectItem>
                    <SelectItem value="urgent">Urgent</SelectItem>
                  </SelectContent>
                </Select>
              </div>

              <div className="space-y-2">
                <Label htmlFor="instructions">Special Instructions</Label>
                <Textarea
                  id="instructions"
                  placeholder="Enter any special instructions for operators..."
                  value={form.instructions || ""}
                  onChange={(e) => updateForm("instructions", e.target.value)}
                  rows={4}
                />
                <p className="text-sm text-muted-foreground">
                  Include setup requirements, quality specifications, safety notes, etc.
                </p>
              </div>

              {form.selectedSalesOrders.length > 0 && (
                <div className="p-4 bg-gray-50 rounded-lg">
                  <h4 className="font-medium mb-2">Order Context:</h4>
                  <div className="space-y-1 text-sm">
                    {form.salesOrders
                      .filter(order => form.selectedSalesOrders.includes(order.id))
                      .map(order => (
                        <div key={order.id} className="flex justify-between">
                          <span>{order.orderNumber} - {order.customerName}</span>
                          <span>Due: {new Date(order.dueDate).toLocaleDateString()}</span>
                        </div>
                      ))}
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        );

      case 5:
        return (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <ListChecks className="h-5 w-5" />
                Select Line Items
              </CardTitle>
              <CardDescription>
                Choose which items from the selected orders to include
              </CardDescription>
            </CardHeader>
            <CardContent>
              <ScrollArea className="max-h-96">
                <div className="space-y-4">
                  {form.availableLineItems.map((lineItem) => {
                    const isSelected = form.selectedLineItems.some(
                      item => item.originalOrderLineItemId === lineItem.id
                    );
                    const salesOrder = form.salesOrders.find(so => so.id === lineItem.salesOrderId);
                    
                    return (
                      <Card 
                        key={lineItem.id}
                        className={`cursor-pointer transition-all hover:shadow-md ${
                          isSelected ? 'ring-2 ring-teal-500 bg-teal-50' : ''
                        }`}
                        onClick={() => toggleLineItem(lineItem)}
                      >
                        <CardContent className="p-4">
                          <div className="flex items-start justify-between">
                            <div className="space-y-2 flex-1">
                              <div className="flex items-center gap-2">
                                <Badge variant="outline" className="text-xs">{lineItem.itemCode}</Badge>
                                {lineItem.isStockItem && (
                                  <Badge variant="secondary" className="text-xs">Stock</Badge>
                                )}
                                <Badge variant="outline" className="text-xs">{salesOrder?.orderNumber}</Badge>
                              </div>
                              <h4 className="font-medium">{lineItem.description}</h4>
                              <p className="text-sm text-muted-foreground">{salesOrder?.customerName}</p>
                              
                              <div className="grid grid-cols-2 md:grid-cols-4 gap-4 text-sm">
                                <div>
                                  <span className="text-muted-foreground">Quantity:</span>
                                  <p className="font-medium">{lineItem.orderQuantity} {lineItem.unit}</p>
                                </div>
                                <div>
                                  <span className="text-muted-foreground">Weight:</span>
                                  <p className="font-medium">{lineItem.orderWeight.toLocaleString()} lbs</p>
                                </div>
                                {lineItem.width && lineItem.length && (
                                  <div>
                                    <span className="text-muted-foreground">Dimensions:</span>
                                    <p className="font-medium">{lineItem.width}\" x {lineItem.length}\"</p>
                                  </div>
                                )}
                                {lineItem.location && (
                                  <div>
                                    <span className="text-muted-foreground">Location:</span>
                                    <p className="font-medium flex items-center gap-1">
                                      <MapPin className="h-3 w-3" />
                                      {lineItem.location}
                                    </p>
                                  </div>
                                )}
                              </div>
                            </div>
                            
                            {isSelected && (
                              <div className="ml-4">
                                <CheckCircle className="h-5 w-5 text-teal-600" />
                              </div>
                            )}
                          </div>
                        </CardContent>
                      </Card>
                    );
                  })}
                </div>
              </ScrollArea>

              {form.selectedLineItems.length > 0 && (
                <div className="mt-6 p-4 bg-teal-50 rounded-lg">
                  <h4 className="font-medium mb-2">Selected Items Summary:</h4>
                  <div className="text-sm text-muted-foreground">
                    {form.selectedLineItems.length} item(s) selected • 
                    Total Weight: {form.selectedLineItems
                      .reduce((sum, item) => sum + (item.orderWeight || 0), 0)
                      .toLocaleString()} lbs
                  </div>
                </div>
              )}
            </CardContent>
          </Card>
        );

      case 6:
        return (
          <Card>
            <CardHeader>
              <CardTitle className="flex items-center gap-2">
                <Eye className="h-5 w-5" />
                Review & Schedule
              </CardTitle>
              <CardDescription>
                Review all details and set the production date
              </CardDescription>
            </CardHeader>
            <CardContent className="space-y-6">
              {/* Scheduling */}
              <div className="space-y-4">
                <h3>Smart Scheduling</h3>
                <div className="space-y-2">
                  <Label htmlFor="scheduledDate">Production Date *</Label>
                  <Input
                    id="scheduledDate"
                    type="date"
                    value={form.scheduledDate}
                    onChange={(e) => updateForm("scheduledDate", e.target.value)}
                  />
                </div>
                
                {form.scheduledStartTime && form.scheduledEndTime && (
                  <div className="p-4 bg-blue-50 rounded-lg">
                    <div className="flex items-center gap-2 mb-2">
                      <Clock className="h-4 w-4 text-blue-600" />
                      <span className="font-medium text-blue-900">Smart Schedule Calculated</span>
                    </div>
                    <div className="grid grid-cols-3 gap-4 text-sm">
                      <div>
                        <span className="text-muted-foreground">Start Time:</span>
                        <p className="font-medium">{form.scheduledStartTime}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">End Time:</span>
                        <p className="font-medium">{form.scheduledEndTime}</p>
                      </div>
                      <div>
                        <span className="text-muted-foreground">Duration:</span>
                        <p className="font-medium">{form.estimatedDuration} hours</p>
                      </div>
                    </div>
                  </div>
                )}
              </div>

              <Separator />

              {/* Review Summary */}
              <div className="grid grid-cols-1 md:grid-cols-2 gap-6">
                <div className="space-y-4">
                  <h3>Work Order Details</h3>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Machine:</span>
                      <span>{form.machine?.name}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Tag Number:</span>
                      <span>{form.tagNumber}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Priority:</span>
                      <Badge className={
                        form.priority === "urgent" ? "bg-red-100 text-red-700" :
                        form.priority === "high" ? "bg-orange-100 text-orange-700" :
                        form.priority === "normal" ? "bg-blue-100 text-blue-700" :
                        "bg-gray-100 text-gray-700"
                      }>
                        {form.priority}
                      </Badge>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Orders:</span>
                      <span>{form.selectedSalesOrders.length}</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Line Items:</span>
                      <span>{form.selectedLineItems.length}</span>
                    </div>
                  </div>
                </div>

                <div className="space-y-4">
                  <h3>Production Summary</h3>
                  <div className="space-y-2 text-sm">
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Total Weight:</span>
                      <span>{form.selectedLineItems
                        .reduce((sum, item) => sum + (item.orderWeight || 0), 0)
                        .toLocaleString()} lbs</span>
                    </div>
                    <div className="flex justify-between">
                      <span className="text-muted-foreground">Machine Capacity:</span>
                      <span>{form.machine?.estimatedLbsPerHour.toLocaleString()} lbs/hr</span>
                    </div>
                    {form.estimatedDuration && (
                      <div className="flex justify-between">
                        <span className="text-muted-foreground">Est. Duration:</span>
                        <span>{form.estimatedDuration} hours</span>
                      </div>
                    )}
                  </div>
                </div>
              </div>

              {form.instructions && (
                <>
                  <Separator />
                  <div className="space-y-2">
                    <h3>Special Instructions</h3>
                    <p className="text-sm text-muted-foreground p-3 bg-gray-50 rounded-lg">
                      {form.instructions}
                    </p>
                  </div>
                </>
              )}

              <Separator />

              <div className="space-y-4">
                <h3>Selected Line Items</h3>
                <div className="space-y-2 max-h-40 overflow-y-auto">
                  {form.selectedLineItems.map((item, index) => (
                    <div key={index} className="flex items-center justify-between p-2 bg-gray-50 rounded text-sm">
                      <div>
                        <p className="font-medium">{item.description}</p>
                        <p className="text-muted-foreground">{item.itemCode} • {item.customerName}</p>
                      </div>
                      <div className="text-right">
                        <p>{item.orderQuantity} {item.unit}</p>
                        <p className="text-muted-foreground">{item.orderWeight?.toLocaleString()} lbs</p>
                      </div>
                    </div>
                  ))}
                </div>
              </div>
            </CardContent>
          </Card>
        );

      default:
        return null;
    }
  };

  return (
    <div className="space-y-6">
      <div className="flex items-center justify-between">
        <div className="flex items-center gap-4">
          <Button 
            variant="ghost" 
            size="sm"
            onClick={() => onNavigate?.("work-orders")}
          >
            <ArrowLeft className="h-4 w-4 mr-2" />
            Back to Work Orders
          </Button>
          <div>
            <h1>Create Work Order</h1>
            <p className="text-muted-foreground">
              Step-by-step work order creation with smart scheduling
            </p>
          </div>
        </div>
      </div>

      {/* Progress Steps */}
      <Card>
        <CardContent className="p-6">
          <div className="flex items-center justify-between">
            {steps.map((step, index) => {
              const isActive = currentStep === step.id;
              const isCompleted = currentStep > step.id;
              const Icon = step.icon;
              
              return (
                <div key={step.id} className="flex items-center">
                  <div className={`flex items-center gap-3 ${
                    index < steps.length - 1 ? 'flex-1' : ''
                  }`}>
                    <div className={`flex items-center justify-center w-10 h-10 rounded-full border-2 ${
                      isCompleted ? 'bg-teal-600 border-teal-600 text-white' :
                      isActive ? 'border-teal-600 text-teal-600' :
                      'border-gray-300 text-gray-400'
                    }`}>
                      {isCompleted ? (
                        <CheckCircle className="h-5 w-5" />
                      ) : (
                        <Icon className="h-5 w-5" />
                      )}
                    </div>
                    <div className="hidden sm:block">
                      <p className={`font-medium ${
                        isActive ? 'text-teal-600' : 
                        isCompleted ? 'text-gray-900' : 'text-gray-400'
                      }`}>
                        {step.name}
                      </p>
                      <p className="text-xs text-muted-foreground">{step.description}</p>
                    </div>
                  </div>
                  {index < steps.length - 1 && (
                    <div className={`flex-1 h-px mx-4 ${
                      isCompleted ? 'bg-teal-600' : 'bg-gray-300'
                    }`} />
                  )}
                </div>
              );
            })}
          </div>
        </CardContent>
      </Card>

      {/* Step Content */}
      <div className="min-h-96">
        {renderStepContent()}
      </div>

      {/* Navigation */}
      <div className="flex items-center justify-between">
        <Button
          variant="outline"
          onClick={prevStep}
          disabled={currentStep === 1}
        >
          <ArrowLeft className="h-4 w-4 mr-2" />
          Previous
        </Button>

        <div className="flex items-center gap-2">
          {currentStep < steps.length ? (
            <Button
              onClick={nextStep}
              disabled={!canProceedToNext()}
              className="bg-teal-600 hover:bg-teal-700"
            >
              Next
              <ArrowRight className="h-4 w-4 ml-2" />
            </Button>
          ) : (
            <Button
              onClick={createWorkOrder}
              disabled={!canProceedToNext()}
              className="bg-teal-600 hover:bg-teal-700"
            >
              <Save className="h-4 w-4 mr-2" />
              Create Work Order
            </Button>
          )}
        </div>
      </div>
    </div>
  );
}