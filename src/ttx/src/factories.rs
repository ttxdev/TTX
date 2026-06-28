use std::collections::HashMap;
use std::sync::Arc;

use crate::jobs::ChatMonitorAdapter;
use crate::platforms::Platform;

pub struct ChatMonitorFactory {
    instances: HashMap<Platform, Arc<dyn ChatMonitorAdapter>>,
}

impl ChatMonitorFactory {
    pub fn new(instances: HashMap<Platform, Arc<dyn ChatMonitorAdapter>>) -> Self {
        Self { instances }
    }

    pub fn create(&self, platform: Platform) -> Option<Arc<dyn ChatMonitorAdapter>> {
        self.instances.get(&platform).cloned()
    }

    pub fn create_all(&self) -> Vec<Arc<dyn ChatMonitorAdapter>> {
        self.instances.values().cloned().collect()
    }
}
